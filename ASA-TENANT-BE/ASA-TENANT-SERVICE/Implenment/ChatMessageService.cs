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
    public class ChatMessageService : IChatMessageService
    {
        private readonly ChatMessageRepo _chatMessageRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public ChatMessageService(ChatMessageRepo chatMessageRepo, IMapper mapper, IUserService userService)
        {
            _chatMessageRepo = chatMessageRepo;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ApiResponse<ChatMessageResponse>> CreateAsync(ChatMessageRequest request)
        {
            try
            {
                var entity = _mapper.Map<ChatMessage>(request);
                var user = await _userService.GetUserbyUserId(request.UserId ?? 0);
                if (user?.ShopId != null)
                {
                    entity.ShopId = user.ShopId;
                }
                var affected = await _chatMessageRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<ChatMessageResponse>(entity);
                    return new ApiResponse<ChatMessageResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ChatMessageResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ChatMessageResponse>
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
                var existing = await _chatMessageRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "ChatMessage not found",
                        Data = false
                    };

                var affected = await _chatMessageRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ChatMessageResponse>> GetFilteredChatMessageAsync(ChatMessageGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<ChatMessage>(Filter);
            var query = _chatMessageRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ChatMessageResponse>
            {
                Items = _mapper.Map<IEnumerable<ChatMessageResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ChatMessageResponse>> UpdateAsync(long id, ChatMessageRequest request)
        {
            try
            {
                var existing = await _chatMessageRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ChatMessageResponse>
                    {
                        Success = false,
                        Message = "ChatMessage not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);
                var user = await _userService.GetUserbyUserId(request.UserId ?? 0);
                if (user?.ShopId != null)
                {
                    existing.ShopId = user.ShopId;
                }
                var affected = await _chatMessageRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ChatMessageResponse>(existing);
                    return new ApiResponse<ChatMessageResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ChatMessageResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ChatMessageResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
