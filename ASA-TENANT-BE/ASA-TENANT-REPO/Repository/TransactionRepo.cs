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
    public class TransactionRepo : GenericRepository<Transaction>
    {
        public TransactionRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Transaction> GetFiltered(Transaction filter)
        {
            var query = _context.Transactions.AsQueryable();

            if (filter.TransactionId > 0)
                query = query.Where(t => t.TransactionId == filter.TransactionId);
            if (filter.OrderId.HasValue && filter.OrderId.Value > 0)
                query = query.Where(t => t.OrderId == filter.OrderId);
            if (filter.UserId.HasValue && filter.UserId.Value > 0)
                query = query.Where(t => t.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(t => t.PaymentStatus == filter.PaymentStatus);
            if (!string.IsNullOrEmpty(filter.AppTransId))
                query = query.Where(t => t.AppTransId == filter.AppTransId);
            if (!string.IsNullOrEmpty(filter.ZpTransId))
                query = query.Where(t => t.ZpTransId == filter.ZpTransId);
            if (filter.ReturnCode.HasValue)
                query = query.Where(t => t.ReturnCode == filter.ReturnCode);
            if (!string.IsNullOrEmpty(filter.ReturnMessage))
                query = query.Where(t => t.ReturnMessage.Contains(filter.ReturnMessage));
            if (filter.CreatedAt.HasValue)
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Date == filter.CreatedAt.Value.Date);

            return query.OrderBy(t => t.TransactionId);
        }
    }
}
