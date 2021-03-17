﻿using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Mapping;
using Turbo.Core.Game.Rooms.Object;
using Turbo.Database.Entities.Room;
using Turbo.Rooms.Managers;
using Turbo.Rooms.Mapping;

namespace Turbo.Rooms
{
    public class Room : IRoom
    {
        public IRoomManager RoomManager { get; private set; }

        public IRoomDetails RoomDetails { get; private set; }
        public IRoomModel RoomModel { get; private set; }
        public IRoomMap RoomMap { get; private set; }

        public IRoomCycleManager RoomCycleManager { get; private set; }
        public IRoomSecurityManager RoomSecurityManager { get; private set; }
        public IRoomFurnitureManager RoomFurnitureManager { get; private set; }
        public IRoomUserManager RoomUserManager { get; private set; }
        private readonly ILogger<IRoom> _logger;

        public bool IsDisposed { get; private set; }
        public bool IsDisposing { get; private set; }

        public Room(IRoomManager roomManager, ILogger<IRoom> logger, RoomEntity roomEntity)
        {
            RoomManager = roomManager;
            _logger = logger;
            RoomDetails = new RoomDetails(roomEntity);

            RoomCycleManager = new RoomCycleManager(this);
            RoomSecurityManager = new RoomSecurityManager(this);
            RoomFurnitureManager = new RoomFurnitureManager(this);
            RoomUserManager = new RoomUserManager(this);
        }

        public async ValueTask InitAsync()
        {
            await LoadMapping();

            if (RoomSecurityManager != null) await RoomSecurityManager.InitAsync();
            if (RoomFurnitureManager != null) await RoomFurnitureManager.InitAsync();
            if (RoomUserManager != null) await RoomUserManager.InitAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (IsDisposing) return;

            IsDisposing = true;

            CancelDispose();

            if (RoomManager != null)
            {
                await RoomManager.RemoveRoom(Id);
            }

            if (RoomCycleManager != null) await RoomCycleManager.DisposeAsync();
            if (RoomUserManager != null) await RoomUserManager.DisposeAsync();
            if (RoomFurnitureManager != null) await RoomFurnitureManager.DisposeAsync();
            if (RoomSecurityManager != null) await RoomSecurityManager.DisposeAsync();
        }

        public void TryDispose()
        {
            if (IsDisposed || IsDisposing) return;

            // if dispose already scheduled, return

            // if has users, return

            // clear the users waiting at the door

            // schedule the dispose to run in 1 minute
        }

        public void CancelDispose()
        {
            // if dispose already scheduled, cancel it
        }

        private async Task LoadMapping()
        {
            if (RoomMap != null)
            {
                RoomMap.Dispose();

                RoomMap = null;
            }

            RoomModel = null;

            IRoomModel roomModel = RoomManager.GetModel(RoomDetails.ModelId);

            if ((roomModel == null) || (!roomModel.DidGenerate)) return;

            RoomModel = roomModel;
            RoomMap = new RoomMap(this);

            RoomMap.GenerateMap();
        }

        public void EnterRoom(IRoomObjectHolder objectHolder)
        {
            if (objectHolder == null) return;
        }

        public async Task Cycle()
        {
            await RoomCycleManager.RunCycles();
        }

        public int Id => RoomDetails.Id;
    }
}
