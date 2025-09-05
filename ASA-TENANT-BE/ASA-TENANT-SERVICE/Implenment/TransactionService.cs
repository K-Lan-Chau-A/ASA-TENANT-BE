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
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionRepo _transactionRepo;
        private readonly IMapper _mapper;
        public TransactionService(TransactionRepo transactionRepo, IMapper mapper)
        {
            _transactionRepo = transactionRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<TransactionResponse>> CreateAsync(TransactionRequest request)
        {
            try
            {
                var entity = _mapper.Map<Transaction>(request);
                var affected = await _transactionRepo.CreateAsync(entity);
                if (affected > 0)
                {
                    var response = _mapper.Map<TransactionResponse>(entity);
                    return new ApiResponse<TransactionResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }
                return new ApiResponse<TransactionResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TransactionResponse>
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
                var existing = await _transactionRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Transaction not found",
                        Data = false
                    };
                }
                var affected = await _transactionRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<TransactionResponse>> GetFilteredTransactionsAsync(TransactionGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Transaction>(Filter);
            var query = _transactionRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<TransactionResponse>
            {
                Items = _mapper.Map<IEnumerable<TransactionResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<TransactionResponse>> UpdateAsync(long id, TransactionRequest request)
        {
            try
            {
                var existing = await _transactionRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<TransactionResponse>
                    {
                        Success = false,
                        Message = "Transaction not found",
                        Data = null
                    };
                }
                _mapper.Map(request, existing);
                var affected = await _transactionRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<TransactionResponse>(existing);
                    return new ApiResponse<TransactionResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }
                return new ApiResponse<TransactionResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TransactionResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
