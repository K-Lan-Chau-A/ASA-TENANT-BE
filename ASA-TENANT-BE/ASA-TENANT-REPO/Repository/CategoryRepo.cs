using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_REPO.Repository
{
    public class CategoryRepo : GenericRepository<Category>
    {
        public CategoryRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<Category> GetFiltered(Category filter)
        {
            var query = _context.Categories.AsQueryable();

            if (filter.CategoryId > 0)
                query = query.Where(c => c.CategoryId == filter.CategoryId);
            if (!string.IsNullOrEmpty(filter.CategoryName))
                query = query.Where(c => c.CategoryName.Contains(filter.CategoryName));

            if (filter.ShopId > 0)
                query = query.Where(c => c.ShopId == filter.ShopId);

            return  query.OrderBy(c => c.CategoryId);
        }
        public async Task<Category> GetByIdAndShopIdAsync(long Categoryid, long shopId)
        {
            return await _context.Categories.FirstOrDefaultAsync(p => p.CategoryId == Categoryid && p.ShopId == shopId);
        }

        public async Task<List<Category>> GetByShopIdAsync(long shopId)
        {
            return await _context.Categories
                .Where(c => c.ShopId == shopId)
                .ToListAsync();
        }
        // Method to check if category has any products
        public async Task<bool> HasProductsAsync(long categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        // Method to get count of products in category
        public async Task<int> GetProductCountAsync(long categoryId)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId);
        }
    }
}
