using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class FcmService : IFcmService
    {
        private readonly FcmRepo _fcmRepo;
        private readonly IMapper _mapper;
        public FcmService(FcmRepo fcmRepo, IMapper mapper)
        {
            _fcmRepo = fcmRepo;
            _mapper = mapper;
        }

        public Task<ApiResponse<FcmResponse>> CreateAsync(FcmRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<FcmResponse>> GetFilteredFcmAsync(FcmGetRequest requestDto, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<FcmResponse>> UpdateAsync(long id, FcmRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
