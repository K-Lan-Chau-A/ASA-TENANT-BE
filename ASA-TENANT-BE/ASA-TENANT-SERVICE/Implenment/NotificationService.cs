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
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepo _notificationRepo;
        private readonly IMapper _mapper;
        public NotificationService(NotificationRepo notificationRepo, IMapper mapper)
        {
            _notificationRepo = notificationRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<NotificationResponse>> CreateAsync(NotificationRequest request)
        {
            try
            {
                var entity = _mapper.Map<Notification>(request);

                var affected = await _notificationRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<NotificationResponse>(entity);
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationResponse>
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
                var existing = await _notificationRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Notification not found",
                        Data = false
                    };

                var affected = await _notificationRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<NotificationResponse>> GetFilteredCategoriesAsync(NotificationGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Notification>(Filter);
            var query = _notificationRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<NotificationResponse>
            {
                Items = _mapper.Map<IEnumerable<NotificationResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<NotificationResponse>> UpdateAsync(long id, NotificationRequest request)
        {
            try
            {
                var existing = await _notificationRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = false,
                        Message = "Notification not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);

                var affected = await _notificationRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<NotificationResponse>(existing);
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
