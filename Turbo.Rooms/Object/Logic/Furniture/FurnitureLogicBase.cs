﻿using Turbo.Core.Game.Furniture.Definition;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Data;
using Turbo.Core.Game.Rooms.Object.Logic;
using Turbo.Rooms.Object.Data;

namespace Turbo.Rooms.Object.Logic.Furniture
{
    public class FurnitureLogicBase : RollingObjectLogic, IFurnitureLogic
    {
        public IFurnitureDefinition FurnitureDefinition { get; private set; }

        public IStuffData StuffData { get; private set; }

        public override void Dispose()
        {
            base.Dispose();

            // ensure furniture saves with data from this logic

            FurnitureDefinition = null;
            _stuffData = null;
        }

        public virtual bool Setup(IFurnitureDefinition furnitureDefinition, string jsonString = null)
        {
            if (furnitureDefinition == null) return false;

            FurnitureDefinition = furnitureDefinition;

            return true;
        }

        protected IStuffData CreateStuffData()
        {
            return StuffDataFactory.CreateStuffData((int)DataKey);
        }

        protected IStuffData CreateStuffDataFromJson(string jsonString)
        {
            return StuffDataFactory.CreateStuffDataFromJson((int)DataKey, jsonString);
        }

        public virtual void OnEnter(IRoomObject roomObject)
        {
            return;
        }

        public virtual void OnLeave(IRoomObject roomObject)
        {
            return;
        }

        public virtual void BeforeStep(IRoomObject roomObject)
        {
            return;
        }

        public virtual void OnStep(IRoomObject roomObject)
        {
            return;
        }

        public virtual void OnStop(IRoomObject roomObject)
        {
            return;
        }

        public virtual void OnInteract(IRoomObject roomObject)
        {
            return;
        }

        public virtual void OnPlace(IRoomManipulator roomManipulator)
        {
            return;
        }

        public virtual void OnMove(IRoomManipulator roomManipulator)
        {
            return;
        }

        public virtual void OnPickup(IRoomManipulator roomManipulator)
        {
            return;
        }

        public virtual bool CanStack() => FurnitureDefinition.CanStack;

        public virtual bool CanWalk() => FurnitureDefinition.CanWalk;

        public virtual bool CanSit() => FurnitureDefinition.CanSit;

        public virtual bool CanLay() => FurnitureDefinition.CanLay;

        public virtual bool CanRoll() => true;

        public virtual bool CanToggle() => true;

        public virtual bool IsOpen() => CanWalk() || CanSit() || CanLay();

        public virtual double StackHeight() => FurnitureDefinition.Z;

        public StuffDataKey DataKey => StuffDataKey.LegacyKey;

        public double Height => RoomObject.Location.Z + StackHeight();
    }
}
