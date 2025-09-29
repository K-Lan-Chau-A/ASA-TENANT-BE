using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Enums;
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
    public class ShiftService : IShiftService
    {
        private readonly ShiftRepo _shiftRepo;
        private readonly IMapper _mapper;
        private readonly OrderRepo _orderRepo;
        public ShiftService(ShiftRepo shiftRepo,IMapper mapper, OrderRepo orderRepo)
        {
            _shiftRepo = shiftRepo;
            _mapper = mapper;
            _orderRepo = orderRepo;
        }

        public async Task<ApiResponse<ShiftResponse>> CreateAsync(ShiftRequest request)
        {
            try
            {
                var entity = _mapper.Map<Shift>(request);

                var affected = await _shiftRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<ShiftResponse>(entity);
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShiftResponse>
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
                var existing = await _shiftRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Shift not found",
                        Data = false
                    };

                var affected = await _shiftRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ShiftResponse>> GetFilteredCategoriesAsync(ShiftGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Shift>(Filter);
            var query = _shiftRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ShiftResponse>
            {
                Items = _mapper.Map<IEnumerable<ShiftResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ShiftResponse>> OpenShift(ShiftOpenRequest shiftOpenRequest)
        {
            try
            {
                // Check nếu shop đã có ca mở thì không cho mở tiếp
                if (await _shiftRepo.HasOpenShiftAsync(shiftOpenRequest.ShopId))
                {
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = false,
                        Message = "This shop already has an open shift. Please close it before opening a new one.",
                        Data = null
                    };
                }

                var entity = _mapper.Map<Shift>(shiftOpenRequest);
                entity.StartDate = DateTime.UtcNow;
                entity.Status = (short)ShiftStatus.Open; // Open
                entity.Revenue = 0;

                var affected = await _shiftRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<ShiftResponse>(entity);
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = true,
                        Message = "Open shift successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = "Open failed",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<ShiftResponse>> CloseShift(ShiftCloseRequest shiftCloseRequest)
        {
            try
            {
                var shift = await _shiftRepo.GetByIdAsync(shiftCloseRequest.ShiftId);
                if (shift == null)
                {
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = false,
                        Message = "Shift not found",
                        Data = null
                    };
                }
                if (shift.Status == (short)ShiftStatus.Closed)
                {
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = false,
                        Message = "Shift already closed",
                        Data = null
                    };
                }
                shift.ClosedDate = DateTime.UtcNow;
                shift.Status = (short)ShiftStatus.Closed;

                //Calculate revenue from orders in this shift
                var revenue = await _orderRepo.GetTotalRevenueByShiftIdAsync(shift.ShiftId);
                shift.Revenue = revenue ?? 0;
                var affected = await _shiftRepo.UpdateAsync(shift);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShiftResponse>(shift);
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = true,
                        Message = "Close shift successfully",
                        Data = response
                    };
                }
                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = "Close shift failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShiftResponse>> UpdateAsync(long id, ShiftRequest request)
        {
            try
            {
                var existing = await _shiftRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = false,
                        Message = "Shift not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);

                var affected = await _shiftRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShiftResponse>(existing);
                    return new ApiResponse<ShiftResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShiftResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
