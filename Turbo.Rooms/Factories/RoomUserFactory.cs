﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Rooms.Managers;

namespace Turbo.Rooms.Factories
{
    public class RoomUserFactory(IServiceProvider _provider) : IRoomUserFactory
    {
        public IRoomUserManager Create(IRoom room)
        {
            return ActivatorUtilities.CreateInstance<RoomUserManager>(_provider, room);
        }
    }
}
