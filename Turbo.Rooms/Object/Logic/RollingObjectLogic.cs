﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turbo.Core.Game.Rooms.Object.Logic;

namespace Turbo.Rooms.Object.Logic
{
    public class RollingObjectLogic : RoomObjectLogicBase, IRollingObjectLogic
    {
        public bool IsRolling { get; protected set; }

        public void ClearRollingData()
        {
            return;
        }
    }
}