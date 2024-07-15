﻿using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turbo.Database.Context;
using Turbo.Database.Entities.Room;

namespace Turbo.Database.Repositories.Room
{
    public class RoomRepository(IEmulatorContext _context) : IRoomRepository
    {
        public async Task<RoomEntity> FindAsync(int id) => await _context.Rooms
            .FirstOrDefaultAsync(room => room.Id == id);
    }
}
