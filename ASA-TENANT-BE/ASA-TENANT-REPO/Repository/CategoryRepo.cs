using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
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
    }
}
