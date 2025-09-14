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
    public class ShopRepo : GenericRepository<Shop>
    {
        public ShopRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Shop> GetFiltered(Shop filter)
        {
            var query = _context.Shops.AsQueryable();
            if (filter.ShopId > 0)
            {
                query = query.Where(s => s.ShopId == filter.ShopId);
            }
            if (!string.IsNullOrEmpty(filter.ShopName))
            {
                query = query.Where(s => s.ShopName.ToLower().Contains(filter.ShopName.ToLower()));
            }
            if (!string.IsNullOrEmpty(filter.Subscription))
            {
                query = query.Where(s => s.Subscription.Contains(filter.Subscription));
            }
            if (!string.IsNullOrEmpty(filter.Address))
            {
                query = query.Where(s => s.Address.Contains(filter.Address));
            }
            if (filter.Status != null)
            {
                query = query.Where(s => s.Status == filter.Status);
            }
            if (!string.IsNullOrEmpty(filter.QrcodeUrl))
            {
                query = query.Where(s => s.QrcodeUrl.Contains(filter.QrcodeUrl));
            }
            return query;
        }

        /// <summary>
        /// Chỉ cập nhật SepayApiKey cho Shop
        /// </summary>
        public async Task<int> UpdateSepayApiKeyAsync(long shopId, string apiKey)
        {
            try
            {
                var shop = await _context.Shops.FindAsync(shopId);
                if (shop == null)
                {
                    return 0;
                }

                shop.SepayApiKey = apiKey;
                _context.Shops.Update(shop);
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
