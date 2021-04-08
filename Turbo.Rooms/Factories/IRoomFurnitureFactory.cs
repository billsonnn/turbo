﻿using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;

namespace Turbo.Rooms.Factories
{
    public interface IRoomFurnitureFactory
    {
        public IRoomFurnitureManager Create(IRoom room);
    }
}
