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
    public class InventoryTransactionRepo : GenericRepository<InventoryTransaction>
    {
        public InventoryTransactionRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<InventoryTransaction> GetFiltered(InventoryTransaction filter)
        {
            var query = _context.InventoryTransactions.AsQueryable();

            if (filter.InventoryTransactionId > 0)
                query = query.Where(it => it.InventoryTransactionId == filter.InventoryTransactionId);
            if (filter.Type > 0)
                query = query.Where(it => it.Type == filter.Type);
            if (filter.ProductId > 0)
                query = query.Where(it => it.ProductId == filter.ProductId);
            if (filter.OrderId > 0)
                query = query.Where(it => it.OrderId == filter.OrderId);
            if (filter.UnitId > 0)
                query = query.Where(it => it.UnitId == filter.UnitId);
            if (filter.Quantity > 0)
                query = query.Where(it => it.Quantity == filter.Quantity);
            if (filter.Price.HasValue)
                query = query.Where(it => it.Price == filter.Price);
            if (filter.ShopId > 0)
                query = query.Where(it => it.ShopId == filter.ShopId);

            return query.OrderBy(it => it.InventoryTransactionId);
        }
    }
}
