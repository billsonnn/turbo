using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turbo.Core.Game.Rooms.Games.Constants;
using Turbo.Rooms.Object.Attributes;

namespace Turbo.Rooms.Object.Logic.Furniture.Games.BattleBanzai.Gates
{
    [RoomObjectLogic("bb_gate_r")]
    public class FurnitureBattleBanzaiGateRedLogic : FurnitureBattleBanzaiGateLogic
    {
        public override GameTeamColor TeamColor => GameTeamColor.Red;
    }
}