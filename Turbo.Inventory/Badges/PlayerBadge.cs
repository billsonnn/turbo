using Turbo.Database.Entities.Players;
using Turbo.Core.Game.Inventory;
using Turbo.Core.Storage;

namespace Turbo.Inventory.Badges
{
    public class PlayerBadge(
        IStorageQueue _storageQueue,
        PlayerBadgeEntity _badgeEntity) : IPlayerBadge
    {

        public void SetSlotId(int? slotId)
        {
            if (_badgeEntity == null) return;

            if (_badgeEntity.SlotId == slotId) return;

            _badgeEntity.SlotId = slotId;

            _storageQueue.Add(_badgeEntity);
        }

        public int Id => _badgeEntity.Id;

        public string BadgeCode => _badgeEntity.BadgeCode;

        public int? SlotId => _badgeEntity.SlotId;
    }
}