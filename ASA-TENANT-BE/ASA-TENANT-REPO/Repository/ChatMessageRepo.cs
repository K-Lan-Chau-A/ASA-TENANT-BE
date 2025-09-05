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
    public class ChatMessageRepo : GenericRepository<ChatMessage>
    {
        public ChatMessageRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<ChatMessage> GetFiltered(ChatMessage filter)
        {
            var query = _context.ChatMessages.AsQueryable();

            if (filter.ChatMessageId > 0)
                query = query.Where(c => c.ChatMessageId == filter.ChatMessageId);
            if (!string.IsNullOrEmpty(filter.Content))
                query = query.Where(c => c.Content.Contains(filter.Content));
            if (filter.ShopId > 0)
                query = query.Where(c => c.ShopId == filter.ShopId);
            if (filter.UserId > 0)
                query = query.Where(c => c.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.Sender))
                query = query.Where(c => c.Sender.Contains(filter.Sender));
            if (filter.CreatedAt != null)
                query = query.Where(c => c.CreatedAt <= filter.CreatedAt);

            return query.OrderBy(c => c.ChatMessageId);
        }
    }
}
