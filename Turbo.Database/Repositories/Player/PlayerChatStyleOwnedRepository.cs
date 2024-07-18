using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Database.Context;
using Turbo.Database.Entities.Players;

namespace Turbo.Database.Repositories.Player
{
    public class PlayerChatStyleOwnedRepository(IEmulatorContext _context) : IPlayerChatStyleOwnedRepository
    {
        public async Task<PlayerChatStyleOwnedEntity> FindAsync(int id) => await _context.PlayerOwnedChatStyles
            .FindAsync(id);

        public async Task<List<PlayerChatStyleOwnedEntity>> FindByPlayerIdAsync(int playerId) => await _context.PlayerOwnedChatStyles
            .Where(e => e.PlayerEntityId == playerId)
            .ToListAsync();

        public async Task AddAsync(PlayerChatStyleOwnedEntity entity)
        {
            _context.PlayerOwnedChatStyles.Add(entity);

            await _context.SaveChangesAsync();
        }
    }
}