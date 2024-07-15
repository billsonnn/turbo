﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Database.Context;
using Turbo.Database.Entities.Room;

namespace Turbo.Database.Repositories.Room
{
    public class RoomRightRepository(IEmulatorContext _context) : IRoomRightRepository
    {
        public async Task<RoomRightEntity> FindAsync(int id) => await _context.RoomRights
            .FirstOrDefaultAsync(entity => entity.Id == id);

        public async Task<List<RoomRightEntity>> FindAllByRoomIdAsync(int roomId) => await _context.RoomRights
            .Where(entity => entity.RoomEntityId == roomId)
            .ToListAsync();

        public async Task<bool> GiveRightsToPlayerIdAsync(int roomId, int playerId)
        {
            var entity = await _context.RoomRights.FirstOrDefaultAsync(entity => (entity.RoomEntityId == roomId) && (entity.PlayerEntityId == playerId));

            if (entity != null) return false;

            entity = new RoomRightEntity();

            entity.RoomEntityId = roomId;
            entity.PlayerEntityId = playerId;

            _context.Add(entity);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveRightsForPlayerIdAsync(int roomId, int playerId)
        {
            var entity = await _context.RoomRights.FirstOrDefaultAsync(entity => (entity.RoomEntityId == roomId) && (entity.PlayerEntityId == playerId));

            if (entity == null) return false;

            _context.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
