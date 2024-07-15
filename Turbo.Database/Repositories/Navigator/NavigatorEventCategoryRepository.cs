﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Database.Context;
using Turbo.Database.Entities.Navigator;

namespace Turbo.Database.Repositories.Navigator
{
    public class NavigatorEventCategoryRepository(IEmulatorContext _context) : INavigatorEventCategoryRepository
    {
        public async Task<NavigatorEventCategoryEntity> FindAsync(int id) => await _context.NavigatorEventCategories.FindAsync(id);

        public async Task<List<NavigatorEventCategoryEntity>> FindAllAsync() => await _context.NavigatorEventCategories.ToListAsync();
    }
}
