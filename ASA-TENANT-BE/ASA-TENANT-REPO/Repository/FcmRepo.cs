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
    public class FcmRepo : GenericRepository<Fcm>
    {
        public FcmRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public async Task<Fcm?> GetFcmByUserIdAndUniqueIdAsync(long userId, string uniqueId)
        {
            return await _context.Fcms
                .FirstOrDefaultAsync(fcm => fcm.UserId == userId && fcm.Uniqueid == uniqueId);
        }
        public async Task<List<Fcm>> GetActiveTokensByUserIdAsync(long userId)
        {
            return await _context.Fcms.Where(fcm => fcm.UserId == userId && fcm.Isactive == true)
                .OrderByDescending(fcm => fcm.Lastlogin)
                .ToListAsync();
        }
        public IQueryable<Fcm> GetFiltered(Fcm filter)
        {
            var query = _context.Fcms.AsQueryable();

            if (filter.FcmId > 0)
                query = query.Where(c => c.FcmId == filter.FcmId);
            if (filter.UserId > 0)
                query = query.Where(c => c.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.FcmToken))
                query = query.Where(c => c.FcmToken.Contains(filter.FcmToken));
            if (!string.IsNullOrEmpty(filter.Uniqueid))
                query = query.Where(c => c.Uniqueid.Contains(filter.Uniqueid));
            if(filter.Isactive.HasValue)
                query = query.Where(c => c.Isactive == filter.Isactive);

            return query.OrderBy(c => c.FcmId);
        }
    }
}
