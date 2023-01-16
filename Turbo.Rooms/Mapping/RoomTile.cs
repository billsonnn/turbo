﻿using System;
using System.Collections.Generic;
using Turbo.Core.Game.Rooms.Mapping;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Logic;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Rooms.Object.Logic.Furniture;
using Turbo.Core.Game;

namespace Turbo.Rooms.Mapping
{
    public class RoomTile : IRoomTile
    {
        public IPoint Location { get; private set; }
        public double DefaultHeight { get; private set; }

        public double Height { get; private set; }
        public int RelativeHeight { get; private set; }
        public RoomTileState State { get; private set; }
        public IRoomObjectFloor HighestObject { get; private set; }

        public List<IRoomObjectAvatar> Avatars { get; private set; }
        public List<IRoomObjectFloor> Furniture { get; private set; }

        public bool IsDoor { get; set; }
        public bool HasStackHelper { get; private set; }

        private double _stackHelperHeight;

        public RoomTile(IPoint location, double height, RoomTileState state)
        {
            Location = location;
            DefaultHeight = height;

            Height = DefaultHeight;
            RelativeHeight = DefaultSettings.TileHeightDefault;
            State = state;

            Avatars = new List<IRoomObjectAvatar>();
            Furniture = new List<IRoomObjectFloor>();

            ResetRelativeHeight();
        }

        public void AddRoomObject(IRoomObject roomObject)
        {
            if (roomObject == null) return;

            if (roomObject is IRoomObjectAvatar avatarObject)
            {
                if (IsDoor) return;

                if (!Avatars.Contains(avatarObject)) Avatars.Add(avatarObject);

                return;
            }

            if (roomObject is IRoomObjectFloor floorObject)
            {
                if (!Furniture.Contains(floorObject)) Furniture.Add(floorObject);

                ResetHighestObject();

                return;
            }
        }

        public void RemoveRoomObject(IRoomObject roomObject)
        {
            if (roomObject == null) return;

            if (roomObject is IRoomObjectAvatar avatarObject)
            {
                if (Avatars.Contains(avatarObject)) Avatars.Remove(avatarObject);

                return;
            }

            if (roomObject is IRoomObjectFloor floorObject)
            {
                if (Furniture.Contains(floorObject)) Furniture.Remove(floorObject);

                ResetHighestObject();

                return;
            }
        }

        public IRoomObjectFloor GetFurnitureUnderneath(IRoomObjectFloor floorObject)
        {
            if (floorObject == null) return null;

            var index = Furniture.IndexOf(floorObject);

            if (index > 0)
            {
                return Furniture[index - 1];
            }

            return null;
        }

        public void ResetTileHeight()
        {
            Height = DefaultHeight;

            if (HighestObject != null)
            {
                Height = HighestObject.Logic.Height;
            }

            ResetRelativeHeight();
        }

        private void ResetHighestObject()
        {
            Height = DefaultHeight;
            HighestObject = null;

            HasStackHelper = false;
            _stackHelperHeight = 0;

            if (Furniture.Count > 0)
            {
                Furniture.Sort((a, b) =>
                {
                    if (a.Logic.Height == b.Logic.Height) return 0;

                    if (b.Logic.Height > a.Logic.Height) return 1;

                    return -1;
                });

                foreach (var floorObject in Furniture)
                {
                    double height = floorObject.Logic.Height;

                    if (floorObject.Logic is FurnitureStackHelperLogic)
                    {
                        HasStackHelper = true;
                        _stackHelperHeight = height;

                        continue;
                    }

                    if (height < Height) continue;

                    HighestObject = floorObject;
                    Height = Math.Round((double)height, 3);
                }
            }

            ResetRelativeHeight();
        }

        private void ResetRelativeHeight()
        {
            RelativeHeight = DefaultSettings.TileHeightDefault;

            if ((State == RoomTileState.Closed) || !CanStack()) return;

            RelativeHeight = (int)Math.Ceiling((decimal)(HasStackHelper ? _stackHelperHeight : Height) * DefaultSettings.TileHeightMultiplier);
        }

        public double GetWalkingHeight()
        {
            double height = Height;

            if (HighestObject != null)
            {
                if (HighestObject.Logic.CanSit() || HighestObject.Logic.CanLay())
                {
                    height -= HighestObject.Logic.StackHeight;
                }
            }

            return Math.Round((double)height, 3);
        }

        public bool HasLogic(Type type)
        {
            foreach (var roomObject in Furniture)
            {
                if (roomObject.Logic.GetType() == type) return true;
            }

            return false;
        }

        public bool CanWalk(IRoomObjectAvatar avatar = null)
        {
            if (State == RoomTileState.Closed) return false;

            if (HighestObject != null)
            {
                if (!HighestObject.Logic.IsOpen(avatar)) return false;

                if (HasStackHelper && (_stackHelperHeight >= HighestObject.Logic.Height)) return false;

                if (!CanSit(avatar) && !CanLay(avatar))
                {
                    var secondHighestObject = GetFurnitureUnderneath(HighestObject);

                    if (secondHighestObject != null)
                    {
                        if ((secondHighestObject.Logic?.Height ?? 0) >= (HighestObject.Location?.Z ?? 0)) return false;
                    }
                }
            }

            if (HasStackHelper) return false;

            return true;
        }

        public bool CanSit(IRoomObjectAvatar avatar = null)
        {
            if (HighestObject != null && HighestObject.Logic.CanSit(avatar)) return true;

            return false;
        }

        public bool CanLay(IRoomObjectAvatar avatar = null)
        {
            if (HighestObject != null && HighestObject.Logic.CanLay(avatar)) return true;

            return false;
        }

        public bool CanStack()
        {
            if (HighestObject != null && !HighestObject.Logic.CanStack()) return false;

            return true;
        }
    }
}
