using System;
using Turbo.Core.Game.Rooms.Object.Logic;

namespace Turbo.Core.Game.Rooms.Object
{
    public interface IRoomObject : IDisposable
    {
        public IRoom Room { get; }

        public int Id { get; }
        public bool NeedsUpdate { get; set; }
        public bool Disposed { get; }
    }
}
