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
    public class CustomerRepo : GenericRepository<Customer>
    {
        public CustomerRepo(ASATENANTDBContext context) : base(context)
        {
        }

        // Override GetByIdAsync để include Rank
        public new async Task<Customer> GetByIdAsync(long? id)
        {
            if (!id.HasValue) return null;
            return await _context.Customers
                .Include(c => c.Rank)
                .FirstOrDefaultAsync(c => c.CustomerId == id.Value);
        }

        public IQueryable<Customer> GetFiltered(Customer filter)
        {
            var query = _context.Customers.Include(c => c.Rank).AsQueryable();

            if (filter.CustomerId > 0)
                query = query.Where(c => c.CustomerId == filter.CustomerId);
            if (!string.IsNullOrEmpty(filter.FullName))
                query = query.Where(c => c.FullName.ToLower().Contains(filter.FullName.ToLower()));
            if (!string.IsNullOrEmpty(filter.Phone))
                query = query.Where(c => c.Phone.Contains(filter.Phone));
            if(!string.IsNullOrEmpty(filter.Email))
                query = query.Where(c => c.Email.Contains(filter.Email));
            if (filter.RankId > 0)
                query = query.Where(c => c.RankId == filter.RankId);
            if (filter.Spent >0 )
                query = query.Where(c => c.Spent == filter.Spent);
            if (filter.ShopId > 0)
                query = query.Where(c => c.ShopId == filter.ShopId);

            return query.OrderBy(c => c.CustomerId);
        }         
    }
}
