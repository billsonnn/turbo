﻿using System;
using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Game.Furniture;
using Turbo.Core.Game.Furniture.Constants;
using Turbo.Core.Game.Furniture.Definition;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Logic;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Rooms.Object.Attributes;

namespace Turbo.Rooms.Object.Logic.Furniture
{
    [RoomObjectLogic("teleport")]
    public class FurnitureTeleportLogic : FurnitureFloorLogic
    {
        private static readonly int _closedState = 0;
        private static readonly int _openState = 1;
        private static readonly int _animatingState = 2;

        private TeleportPairingDto _dto;
        private IRoomObjectAvatar _pendingAvatar;
        private IPlayer _pendingPlayer;
        private bool _didFindTeleport;
        private bool _needsOpening;
        private bool _needsClosing;
        private bool _needsAnimating;

        public override async Task<bool> Setup(IFurnitureDefinition furnitureDefinition, string jsonString = null)
        {
            if (!await base.Setup(furnitureDefinition, jsonString)) return false;

            SetState(_closedState);

            if (_dto == null)
            {
                if (RoomObject.RoomObjectHolder is IRoomFloorFurniture furniture)
                {
                    _dto = await furniture.GetTeleportPairing();
                }
            }

            return true;
        }

        public override void Dispose()
        {
            if (_pendingAvatar != null)
            {
                _pendingAvatar.Logic.CanWalk = true;

                _pendingAvatar = null;
            }

            if (_pendingPlayer != null) _pendingPlayer = null;

            base.Dispose();
        }

        public override async Task Cycle()
        {
            await base.Cycle();

            if (_needsOpening)
            {
                SetState(_openState);

                _needsOpening = false;
            }

            if (_needsClosing)
            {
                SetState(_closedState);

                _needsClosing = false;
            }

            if (_needsAnimating)
            {
                SetState(_animatingState);

                _needsAnimating = false;
            }

            if (_pendingPlayer != null)
            {
                await ReceiveTeleport(_pendingPlayer);
            }

            if (_pendingAvatar == null) return;

            if (_pendingAvatar.Disposed)
            {
                SetState(_closedState);

                _pendingAvatar = null;

                return;
            }

            if (!_pendingAvatar.Location.Compare(RoomObject.Location))
            {
                if (_pendingAvatar.Logic.IsWalking && _pendingAvatar.Logic.LocationGoal.Compare(RoomObject.Location)) return;

                _pendingAvatar.Logic.CanWalk = true;

                SetState(_closedState);

                _pendingAvatar = null;

                return;
            }

            IRoom remoteRoom = null;

            if (_dto != null && _dto.RoomId != null)
            {
                remoteRoom = await RoomObject.Room.RoomManager.GetRoom((int)_dto.RoomId);
            }

            bool didFail = false;

            if (_dto == null || remoteRoom == null)
            {
                didFail = true;
            }
            else
            {
                if (remoteRoom.Id != RoomObject.Room.Id) await remoteRoom.InitAsync();

                int state = StuffData.GetState();

                if (state == _openState)
                {
                    _needsClosing = true;

                    return;
                }

                else if (state == _closedState)
                {
                    _needsAnimating = true;

                    return;
                }

                else if (state == _animatingState)
                {
                    var remoteFurniture = remoteRoom.RoomFurnitureManager.GetFloorFurniture(_dto.TeleportId);

                    if (!_didFindTeleport)
                    {
                        if (remoteFurniture == null)
                        {
                            didFail = true;
                        }
                        else
                        {
                            _didFindTeleport = true;

                            return;
                        }
                    }

                    if (remoteFurniture == null || remoteFurniture.RoomObject == null)
                    {
                        didFail = true;
                    }

                    else if ((remoteFurniture.RoomObject is IRoomObjectFloor floorObject) && floorObject.Logic is FurnitureTeleportLogic teleportLogic)
                    {
                        if (_pendingAvatar != null && teleportLogic.StuffData.GetState() != _animatingState)
                        {
                            teleportLogic.SetState(_animatingState);

                            return;
                        }

                        SetState(_closedState);

                        if (_pendingAvatar.RoomObjectHolder is IPlayer player)
                        {
                            await teleportLogic.ReceiveTeleport(player);
                        }

                        _pendingAvatar = null;
                    }

                    else
                    {
                        didFail = true;
                    }
                }
            }

            if (didFail)
            {

                _pendingAvatar.Logic.WalkTo(RoomObject.Location.GetPointForward());

                _pendingAvatar.Logic.CanWalk = true;

                _pendingAvatar = null;

                _needsClosing = true;

                return;
            }
        }

        private async Task ReceiveTeleport(IPlayer player)
        {
            if (player.RoomObject == null || player.RoomObject.Disposed)
            {
                _pendingAvatar = null;

                _needsClosing = true;

                return;
            }

            _pendingPlayer = player;

            if (player.RoomObject != null)
            {
                if (player.RoomObject.Room != RoomObject.Room)
                {
                    await player.PlayerManager.EnterRoom(player, RoomObject.Room.Id, null, true, RoomObject.Location);
                }
                else
                {
                    if (player.RoomObject == null || player.RoomObject.Disposed)
                    {
                        _pendingPlayer = null;

                        _needsClosing = true;

                        return;
                    }

                    player.RoomObject.Logic.CanWalk = false;

                    if (player.RoomObject.Location.Compare(RoomObject.Location))
                    {
                        if (StuffData.GetState() != _openState)
                        {
                            _needsOpening = true;

                            return;
                        }

                        if (StuffData.GetState() == _openState)
                        {
                            player.RoomObject.Logic.WalkTo(RoomObject.Location.GetPointForward());

                            player.RoomObject.Logic.CanWalk = true;

                            _pendingPlayer = null;
                        }

                        _needsClosing = true;
                    }
                    else
                    {
                        player.RoomObject.Logic.GoTo(RoomObject.Location);
                    }
                }
            }
        }

        public override void OnInteract(IRoomObjectAvatar avatar, int param = 0)
        {
            if (_pendingAvatar != null && avatar != _pendingAvatar) return;

            IPoint goalPoint = GetGoalPoint();

            if (!avatar.Location.Compare(goalPoint))
            {
                _pendingAvatar.Logic.WalkTo(goalPoint, true);

                _pendingAvatar.Logic.BeforeGoalAction = new Action<IRoomObjectAvatar>(p => OnInteract(p));

                return;
            }

            SetState(_openState);

            _pendingAvatar.Logic.CanWalk = false;

            _pendingAvatar = avatar;

            if (avatar.Location.Compare(RoomObject.Location)) return;

            _pendingAvatar.Logic.WalkTo(RoomObject.Location);
        }

        public override void OnPickup(IRoomManipulator roomManipulator)
        {
            SetState(_closedState);

            base.OnPickup(roomManipulator);
        }

        public override bool CanWalk(IRoomObjectAvatar avatar = null)
        {
            if ((_pendingAvatar != null) && avatar == _pendingAvatar) return true;

            return base.CanWalk(avatar);
        }

        private IPoint GetGoalPoint()
        {
            IPoint goalPoint;

            if (CanWalk())
            {
                goalPoint = RoomObject.Location;
            }
            else
            {
                goalPoint = RoomObject.Location.GetPointForward();
            }

            return goalPoint;
        }

        public override FurniUsagePolicy UsagePolicy => FurniUsagePolicy.Everybody;
    }
}
