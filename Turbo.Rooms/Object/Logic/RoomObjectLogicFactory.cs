﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Turbo.Core.Events;
using Turbo.Core.Game.Furniture.Data;
using Turbo.Core.Game.Rooms.Object.Logic;
using Turbo.Rooms.Object.Attributes;
using Turbo.Rooms.Object.Logic.Avatar;
using Turbo.Rooms.Object.Logic.Furniture;
using Turbo.Rooms.Object.Logic.Furniture.Games.BattleBanzai;
using Turbo.Rooms.Object.Logic.Furniture.Games.BattleBanzai.Gates;
using Turbo.Rooms.Object.Logic.Furniture.Games.BattleBanzai.ScoreBoards;

namespace Turbo.Rooms.Object.Logic
{
    public class RoomObjectLogicFactory : IRoomObjectLogicFactory
    {
        private readonly ITurboEventHub _eventHub;
        public IDictionary<string, Type> Logics { get; } = new Dictionary<string, Type>();


        public RoomObjectLogicFactory(ITurboEventHub eventHub)
        {
            _eventHub = eventHub;

            foreach (var item in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(RoomObjectLogicAttribute))))
            {
                var attributeData = item.GetCustomAttribute<RoomObjectLogicAttribute>();

                if (attributeData == null) continue;

                Logics.TryAdd(attributeData.Name, item);
            }
        }

        public IRoomObjectLogic Create(string type)
        {
            var logicType = GetLogicType(type);

            if (logicType == null) return null;

            var instance = (IRoomObjectLogic)Activator.CreateInstance(logicType);

            instance.SetEventHub(_eventHub);

            return instance;
        }

        public Type GetLogicType(string type)
        {
            if (!Logics.TryGetValue(type, out Type value)) return null;

            return value;
        }

        public StuffDataKey GetStuffDataKeyForFurnitureType(string type)
        {
            if (!Logics.TryGetValue(type, out Type value)) return StuffDataKey.LegacyKey;

            if (value.IsAssignableFrom(typeof(FurnitureLogicBase)))
            {
                //var logicType = typeof(FurnitureLogicBase) Logics[type];
            }

            return StuffDataKey.LegacyKey;
        }
    }
}
