using System;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Game.Furniture;
using Turbo.Core.Game.Furniture.Definition;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Core.Game.Rooms.Object.Constants;
using Turbo.Database.Entities.Furniture;
using Turbo.Core.Game.Players;

namespace Turbo.Furniture
{
    public abstract class RoomFurniture(
        ILogger<IRoomFurniture> _logger,
        IRoomFurnitureManager _roomFurnitureManager,
        IFurnitureManager _furnitureManager,
        FurnitureEntity _furnitureEntity,
        IFurnitureDefinition _furnitureDefinition) : IRoomFurniture
    {
        public FurnitureEntity FurnitureEntity { get; } = _furnitureEntity;
        public IFurnitureDefinition FurnitureDefinition { get; } = _furnitureDefinition;
        public string PlayerName { get; private set; }

        private IRoom _room;
        private bool _isDisposing { get; set; }

        public void Dispose()
        {
            if (_isDisposing) return;

            _isDisposing = true;

            OnDispose();
        }

        protected abstract void OnDispose();

        public void Save()
        {
            OnSave();
        }

        protected abstract void OnSave();

        public void SetRoom(IRoom room)
        {
            if (room == null)
            {
                _room = null;

                if (FurnitureEntity.RoomEntityId != null)
                {
                    FurnitureEntity.RoomEntityId = null;

                    Save();
                }

                return;
            }

            _room = room;

            if (FurnitureEntity.RoomEntityId == room.Id) return;

            FurnitureEntity.RoomEntityId = room.Id;

            Save();
        }

        public bool SetPlayer(IPlayer player)
        {
            if (player == null) return false;

            return SetPlayer(player.Id, player.Name);
        }

        public bool SetPlayer(int playerId, string playerName = "")
        {
            if (playerId <= 0) return false;

            if (FurnitureEntity.PlayerEntityId != playerId)
            {
                FurnitureEntity.PlayerEntityId = playerId;

                Save();
            }

            if (playerName.Length > 0) PlayerName = playerName;

            return true;
        }

        public int Id => FurnitureEntity.Id;
        public RoomObjectHolderType Type => RoomObjectHolderType.Furniture;
        public string LogicType => FurnitureDefinition.Logic;
        public int PlayerId => FurnitureEntity.PlayerEntityId;
    }
}
