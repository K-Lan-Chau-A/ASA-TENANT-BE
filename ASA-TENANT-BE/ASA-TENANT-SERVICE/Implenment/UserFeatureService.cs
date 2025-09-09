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
    public class UserFeatureService : IUserFeatureService
    {
        private readonly UserFeatureRepo _userFeatureRepo;
        private readonly IMapper _mapper;
        public UserFeatureService(UserFeatureRepo userFeatureRepo, IMapper mapper)
        {
            _userFeatureRepo = userFeatureRepo;
            _mapper = mapper;
        }
    }
}
