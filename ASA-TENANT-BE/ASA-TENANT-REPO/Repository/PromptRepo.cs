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
    public class PromptRepo : GenericRepository<Prompt>
    {
        public PromptRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<Prompt> GetFiltered(Prompt filter)
        {
            var query = _context.Prompts.AsQueryable();
            if (filter.PromptId > 0)
                query = query.Where(p => p.PromptId == filter.PromptId);
            if (!string.IsNullOrEmpty(filter.Title))
                query = query.Where(p => p.Title.Contains(filter.Title));
            if (!string.IsNullOrEmpty(filter.Content))
                query = query.Where(p => p.Content.Contains(filter.Content));
            if (!string.IsNullOrEmpty(filter.Description))
                query = query.Where(p => p.Description.Contains(filter.Description));
            return query.OrderBy(p => p.PromptId);
        }
    }
}
