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
    public class ProductRepo : GenericRepository<Product>
    {
        public ProductRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Product> GetFiltered(Product filter)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();
            if (filter.ProductId > 0)
                query = query.Where(p => p.ProductId == filter.ProductId);
            if (!string.IsNullOrEmpty(filter.ProductName))
                query = query.Where(p => p.ProductName.Contains(filter.ProductName));
            if (filter.CategoryId > 0)
                query = query.Where(p => p.CategoryId == filter.CategoryId);
            if (filter.ShopId > 0)
                query = query.Where(p => p.ShopId == filter.ShopId);
            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status);
            if (filter.UnitIdFk.HasValue)
                query = query.Where(p => p.UnitIdFk == filter.UnitIdFk);
            if (filter.Price > 0)
                query = query.Where(p => p.Price == filter.Price);
            if (filter.Cost > 0)
                query = query.Where(p => p.Cost == filter.Cost);
            if (filter.Discount > 0)
                query = query.Where(p => p.Discount == filter.Discount);
            if (filter.Quantity > 0)
                query = query.Where(p => p.Quantity == filter.Quantity);
            if (!string.IsNullOrEmpty(filter.Barcode))
                query = query.Where(p => p.Barcode == filter.Barcode);
            return query.OrderBy(p => p.ProductId);
        }
        public async Task<Product> GetByBarcodeAsync(string barcode, long shopId)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode && p.ShopId == shopId);
        }
        public async Task<bool> UnActiveProduct(long productId, long shopId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

            if (product == null)
                return false;

            product.Status = 0; 
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
