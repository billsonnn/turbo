﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Logic.Wired;

namespace Turbo.Rooms.Object.Logic.Furniture.Wired.Arguments
{
    public class WiredArguments : IWiredArguments
    {
        public IRoomObject UserObject { get; set; }
        public IRoomObject FurnitureObject { get; set; }
    }
}