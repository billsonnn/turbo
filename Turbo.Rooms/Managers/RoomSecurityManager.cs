﻿using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Constants;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Constants;
using Turbo.Core.Game.Rooms.Object.Logic;
using Turbo.Core.Packets.Messages;
using Turbo.Packets.Outgoing.Room.Engine;

namespace Turbo.Rooms.Managers
{
    public class RoomSecurityManager : IRoomSecurityManager
    {
        private readonly IRoom _room;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private IDictionary<int, string> _rights;

        public RoomSecurityManager(
            IRoom room,
            IServiceScopeFactory serviceScopeFactory)
        {
            _room = room;
            _serviceScopeFactory = serviceScopeFactory;

            _rights = new Dictionary<int, string>();
        }

        public async ValueTask InitAsync()
        {

        }

        public async ValueTask DisposeAsync()
        {

        }

        public bool IsOwner(IRoomManipulator manipulator)
        {
            if (IsStrictOwner(manipulator)) return true;

            if (manipulator.HasPermission("any_room_owner")) return true;

            return false;
        }

        public bool IsStrictOwner(IRoomManipulator manipulator)
        {
            if (_room.RoomDetails.PlayerId == manipulator.Id) return true;

            return false;
        }

        public bool IsController(IRoomManipulator manipulator)
        {
            if (IsOwner(manipulator)) return true;

            if (manipulator.HasPermission("any_room_rights")) return true;

            if (_rights.ContainsKey(manipulator.Id)) return true;

            return false;
        }

        public void RefreshControllerLevel(IRoomObject roomObject)
        {
            bool isOwner = false;
            RoomControllerLevel controllerLevel = RoomControllerLevel.None;

            if (roomObject.RoomObjectHolder is IPlayer player)
            {
                if (IsOwner(player))
                {
                    isOwner = true;
                    controllerLevel = RoomControllerLevel.Moderator;
                }

                else if (IsController(player))
                {
                    controllerLevel = RoomControllerLevel.Rights;
                }

                // composer 780 roomrights

                player.Session.Send(new RoomEntryInfoMessage
                {
                    RoomId = _room.Id,
                    Owner = isOwner
                });

                // composer 339 room owner
            }

            if (roomObject.Logic is IMovingAvatarLogic avatarLogic)
            {
                avatarLogic.AddStatus(RoomObjectAvatarStatus.FlatControl, ((int)controllerLevel).ToString());
            }
        }

        public void SendOwnersComposer(IComposer composer)
        {
            foreach (IRoomObject roomObject in _room.RoomUserManager.RoomObjects.Values)
            {
                if (roomObject.RoomObjectHolder is IPlayer player)
                {
                    if (!IsOwner(player)) continue;

                    player.Session.Send(composer);
                }
            }
        }

        public void SendRightsComposer(IComposer composer)
        {
            foreach (IRoomObject roomObject in _room.RoomUserManager.RoomObjects.Values)
            {
                if (roomObject.RoomObjectHolder is IPlayer player)
                {
                    if (!IsController(player)) continue;

                    player.Session.Send(composer);
                }
            }
        }
    }
}
