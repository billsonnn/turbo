using Turbo.Core.Game.Rooms.Utils;
using Turbo.Core.Game.Rooms.Object.Logic;

namespace Turbo.Core.Game.Rooms.Object
{
    public interface IRoomObjectAvatar : IRoomObject
    {
        public IRoomObjectAvatarHolder RoomObjectHolder { get; }
        public IMovingAvatarLogic Logic { get; }
        public IPoint Location { get; }

        public bool SetHolder(IRoomObjectAvatarHolder roomObjectHolder);
        public void SetLogic(IMovingAvatarLogic logic);
        public void SetLocation(IPoint point);

        public int X { get; set; }
        public int Y { get; set; }
        public double Z { get; set; }
        public Rotation Rotation { get; set; }
        public Rotation HeadRotation { get; set; }
    }
}