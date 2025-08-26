using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class PromotionProductService : IPromotionProductService
    {
        private readonly PromotionProductRepo _promotionProductRepo;
        public PromotionProductService(PromotionProductRepo promotionProductRepo)
        {
            _promotionProductRepo = promotionProductRepo;
        }
    }
}
