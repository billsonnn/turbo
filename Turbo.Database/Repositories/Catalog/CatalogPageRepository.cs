using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Database.Context;
using Turbo.Database.Entities.Catalog;

namespace Turbo.Database.Repositories.Catalog
{
    public class CatalogPageRepository(IEmulatorContext _context) : ICatalogPageRepository
    {
        public async Task<CatalogPageEntity> FindAsync(int id) => await _context.CatalogPages
            .FirstOrDefaultAsync(page => page.Id == id);

        public async Task<List<CatalogPageEntity>> FindAllAsync() => await _context.CatalogPages
            .AsNoTracking()
            .ToListAsync();
    }
}