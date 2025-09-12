using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ShopSubscriptionService : IShopSubscriptionService
    {
        private readonly ShopSubscriptionRepo _shopSubscriptionRepo;
        private readonly IMapper _mapper;
        public ShopSubscriptionService(ShopSubscriptionRepo shopSubscriptionRepo, IMapper mapper)
        {
            _shopSubscriptionRepo = shopSubscriptionRepo;
            _mapper = mapper;
        }
    }
}
