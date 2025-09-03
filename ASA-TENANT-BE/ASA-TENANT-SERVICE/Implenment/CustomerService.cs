using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerRepo _customerRepo;
        private readonly IMapper _mapper;
        public CustomerService(CustomerRepo customerRepo, IMapper mapper)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CustomerResponse>> CreateAsync(CustomerRequest request)
        {
            try
            {
                var entity = _mapper.Map<Customer>(request);

                var affected = await _customerRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<CustomerResponse>(entity);
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _customerRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = false
                    };

                var affected = await _customerRepo.RemoveAsync(existing);
                return new ApiResponse<bool>
                {
                    Success = affected,
                    Message = affected ? "Deleted successfully" : "Delete failed",
                    Data = affected
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<PagedResponse<CustomerResponse>> GetFilteredCustomersAsync(CustomerGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Customer>(Filter);

            var query = _customerRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResponse<CustomerResponse>
            {
                Items = _mapper.Map<IEnumerable<CustomerResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

        }

        public async Task<ApiResponse<CustomerResponse>> UpdateAsync(long id, CustomerRequest request)
        {
            try
            {
                var existing = await _customerRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                var affected = await _customerRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<CustomerResponse>(existing);
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
