﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Mapping;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Constants;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Core.Packets.Messages;
using Turbo.Packets.Outgoing.Room.Engine;
using Turbo.Rooms.Object.Logic.Avatar;

namespace Turbo.Rooms.Managers
{
    public class RoomUserManager : IRoomUserManager
    {
        private readonly IRoom _room;

        public IDictionary<int, IRoomObject> RoomObjects { get; private set; }

        private int _roomObjectCounter;

        public RoomUserManager(IRoom room)
        {
            _room = room;

            RoomObjects = new Dictionary<int, IRoomObject>();

            _roomObjectCounter = -1;
        }

        public async ValueTask InitAsync()
        {

        }

        public async ValueTask DisposeAsync()
        {
            RemoveAllRoomObjects();
        }

        public IRoomObject GetRoomObject(int id)
        {
            if (id < 0) return null;

            if (RoomObjects.TryGetValue(id, out IRoomObject roomObject))
            {
                return roomObject;
            }

            return null;
        }

        public IRoomObject GetRoomObjectByUserId(int userId)
        {
            return null;
        }

        public IRoomObject GetRoomObjectByUsername(string username)
        {
            return null;
        }

        public IRoomObject AddRoomObject(IRoomObject roomObject, IPoint location = null)
        {
            if (roomObject == null) return null;

            IRoomObject existingRoomObject = GetRoomObject(roomObject.Id);

            if (existingRoomObject != null)
            {
                roomObject.Dispose();

                return null;
            }

            if (roomObject.Logic is not MovingAvatarLogic avatarLogic) return null;

            if (location == null) location = _room.RoomModel.DoorLocation.Clone();

            roomObject.SetLocation(location);

            _room.RoomMap.AddRoomObjects(roomObject);

            RoomObjects.Add(roomObject.Id, roomObject);

            UpdateTotalUsers();

            return roomObject;
        }

        public IRoomObject CreateRoomObjectAndAssign(IRoomObjectFactory objectFactory, IRoomObjectUserHolder userHolder, IPoint location)
        {
            if (userHolder == null) return null;

            IRoomObject roomObject = objectFactory.Create(_room, this, ++_roomObjectCounter, userHolder.Type, userHolder.Type);

            if (roomObject == null) return null;

            if (!userHolder.SetRoomObject(roomObject)) return null;

            return AddRoomObject(roomObject, location);
        }

        public void RemoveRoomObject(int id)
        {
            IRoomObject roomObject = GetRoomObject(id);

            if (roomObject == null) return;

            if (roomObject.Logic is MovingAvatarLogic avatarLogic)
            {
                avatarLogic.GetCurrentTile()?.RemoveRoomObject(roomObject);
                avatarLogic.GetNextTile()?.RemoveRoomObject(roomObject);

                avatarLogic.StopWalking();
            }

            SendComposer(new UserRemoveMessage
            {
                Id = id
            });

            RoomObjects.Remove(id);

            // if the room object was playing a game, remove it from that game

            roomObject.Dispose();

            UpdateTotalUsers();

            _room.TryDispose();
        }

        public void RemoveAllRoomObjects()
        {
            foreach (int id in RoomObjects.Keys) RemoveRoomObject(id);
        }

        public void EnterRoom(IRoomObjectFactory objectFactory, IPlayer player, IPoint location = null)
        {
            if ((objectFactory == null) || (player == null)) return;

            CreateRoomObjectAndAssign(objectFactory, player, location);

            IList<IRoomObject> roomObjects = new List<IRoomObject>();
            IList<IComposer> composers = new List<IComposer>();

            foreach (IRoomObject existingObject in RoomObjects.Values)
            {
                roomObjects.Add(existingObject);

                if (existingObject.Logic is AvatarLogic avatarLogic)
                {
                    RoomObjectAvatarDanceType danceType = avatarLogic.DanceType;
                    bool isIdle = avatarLogic.IsIdle;

                    // if(danceType > RoomObjectAvatarDanceType.None)
                    // if(idIdle)
                }
            }

            player.Session.Send(new UsersMessage
            {
                RoomObjects = roomObjects
            });

            player.Session.Send(new UserUpdateMessage
            {
                RoomObjects = roomObjects
            });

            foreach (IComposer composer in composers) player.Session.SendQueue(composer);
        }

        private void UpdateTotalUsers()
        {
            int totalUsers = 0;

            foreach(IRoomObject roomObject in RoomObjects.Values)
            {
                if (!roomObject.Type.Equals("user")) continue;

                totalUsers++;
            }

            _room.RoomDetails.UsersNow = totalUsers;
        }

        public void SendComposer(IComposer composer)
        {
            _room.SendComposer(composer);
        }
    }
}
