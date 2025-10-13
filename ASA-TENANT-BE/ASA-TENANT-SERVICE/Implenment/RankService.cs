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
    public class RankService : IRankService
    {
        private readonly RankRepo _rankRepo;
        private readonly IMapper _mapper;
        public RankService(RankRepo rankRepo, IMapper mapper)
        {
            _rankRepo = rankRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<RankResponse>> CreateAsync(RankRequest request)
        {
            try
            {
                // Validation 0: Check benefit range (0-1)
                if (request.Benefit.HasValue && (request.Benefit.Value < 0 || request.Benefit.Value > 1))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Benefit value must be between 0 and 1",
                        Data = null
                    };
                }

                // Validation 0: Check threshold is not negative
                if (request.Threshold.HasValue && request.Threshold.Value < 0)
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Threshold value cannot be negative",
                        Data = null
                    };
                }

                // Validation 1: Check if rank name already exists in the same shop
                if (await _rankRepo.IsRankNameExistsAsync(request.RankName, request.ShopId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Rank name already exists in this shop",
                        Data = null
                    };
                }

                // Validation 2: Check if benefit already exists in the same shop
                if (request.Benefit.HasValue && await _rankRepo.IsBenefitExistsAsync(request.Benefit.Value, request.ShopId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Benefit value already exists in this shop",
                        Data = null
                    };
                }

                // Validation 3: Check if threshold already exists in the same shop (only for non-null thresholds)
                if (request.Threshold.HasValue && await _rankRepo.IsThresholdExistsAsync(request.Threshold.Value, request.ShopId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Threshold value already exists in this shop",
                        Data = null
                    };
                }

                // Validation 3.5: Handle null threshold logic
                if (!request.Threshold.HasValue)
                {
                    // Check if there's already a rank with null threshold in this shop
                    if (await _rankRepo.HasRankWithNullThresholdAsync(request.ShopId))
                    {
                        var existingNullThresholdRank = await _rankRepo.GetRankWithNullThresholdAsync(request.ShopId);
                        var maxBenefit = await _rankRepo.GetMaxBenefitInShopAsync(request.ShopId);
                        
                        // If new rank has benefit and it's not the highest, reject
                        if (request.Benefit.HasValue && maxBenefit.HasValue && request.Benefit.Value < maxBenefit.Value)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot create rank with null threshold: there's already a rank with null threshold (ID: {existingNullThresholdRank.RankId}) and the new rank's benefit ({request.Benefit.Value}) is not the highest. Only the rank with the highest benefit can have null threshold.",
                                Data = null
                            };
                        }
                        
                        // If new rank has no benefit or benefit is the highest, we need to update existing null threshold rank
                        if (request.Benefit.HasValue && (!maxBenefit.HasValue || request.Benefit.Value >= maxBenefit.Value))
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot create rank with null threshold: there's already a rank with null threshold (ID: {existingNullThresholdRank.RankId}). Please update the existing rank instead of creating a new one.",
                                Data = null
                            };
                        }
                    }
                }
                else
                {
                    // If new rank has threshold, check if it should have the highest benefit
                    if (request.Benefit.HasValue)
                    {
                        var existingNullThresholdRank = await _rankRepo.GetRankWithNullThresholdAsync(request.ShopId);
                        if (existingNullThresholdRank != null && existingNullThresholdRank.Benefit.HasValue && request.Benefit.Value > existingNullThresholdRank.Benefit.Value)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot create rank: the new rank's benefit ({request.Benefit.Value}) is higher than the existing null threshold rank's benefit ({existingNullThresholdRank.Benefit.Value}). The rank with highest benefit must have null threshold.",
                                Data = null
                            };
                        }
                    }
                }

                // Validation 4: Check threshold-benefit consistency
                if (request.Threshold.HasValue && request.Benefit.HasValue)
                {
                    var existingRanks = await _rankRepo.GetRanksByShopIdAsync(request.ShopId);
                    
                    // Check if new rank's benefit is less than any rank with lower threshold
                    var conflictingRank = existingRanks.FirstOrDefault(r => 
                        r.Threshold < request.Threshold.Value && 
                        r.Benefit.HasValue && 
                        r.Benefit.Value > request.Benefit.Value);
                    
                    if (conflictingRank != null)
                    {
                        return new ApiResponse<RankResponse>
                        {
                            Success = false,
                            Message = $"Benefit value ({request.Benefit.Value}) cannot be less than benefit ({conflictingRank.Benefit.Value}) of rank with lower threshold ({conflictingRank.Threshold.Value})",
                            Data = null
                        };
                    }

                    // Check if any existing rank with higher threshold has benefit less than new rank's benefit
                    var higherThresholdRank = existingRanks.FirstOrDefault(r => 
                        r.Threshold > request.Threshold.Value && 
                        r.Benefit.HasValue && 
                        r.Benefit.Value < request.Benefit.Value);
                    
                    if (higherThresholdRank != null)
                    {
                        return new ApiResponse<RankResponse>
                        {
                            Success = false,
                            Message = $"Cannot create rank: existing rank with higher threshold ({higherThresholdRank.Threshold.Value}) has lower benefit ({higherThresholdRank.Benefit.Value}) than this rank's benefit ({request.Benefit.Value})",
                            Data = null
                        };
                    }
                }

                var entity = _mapper.Map<Rank>(request);

                var affected = await _rankRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<RankResponse>(entity);
                    return new ApiResponse<RankResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<RankResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<RankResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var existing = await _rankRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Rank not found",
                        Data = false
                    };

                var affected = await _rankRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<RankResponse>> GetFilteredUnitsAsync(RankGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Rank>(Filter);
            var query = _rankRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<RankResponse>
            {
                Items = _mapper.Map<IEnumerable<RankResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<RankResponse>> UpdateAsync(int id, RankRequest request)
        {
            try
            {
                var existing = await _rankRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Rank not found",
                        Data = null
                    };

                // Validation 0: Check benefit range (0-1)
                if (request.Benefit.HasValue && (request.Benefit.Value < 0 || request.Benefit.Value > 1))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Benefit value must be between 0 and 1",
                        Data = null
                    };
                }

                // Validation 0: Check threshold is not negative
                if (request.Threshold.HasValue && request.Threshold.Value < 0)
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Threshold value cannot be negative",
                        Data = null
                    };
                }

                // Validation 1: Check if rank name already exists in the same shop (excluding current rank)
                if (await _rankRepo.IsRankNameExistsAsync(request.RankName, request.ShopId, existing.RankId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Rank name already exists in this shop",
                        Data = null
                    };
                }

                // Validation 2: Check if benefit already exists in the same shop (excluding current rank)
                if (request.Benefit.HasValue && await _rankRepo.IsBenefitExistsAsync(request.Benefit.Value, request.ShopId, existing.RankId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Benefit value already exists in this shop",
                        Data = null
                    };
                }

                // Validation 3: Check if threshold already exists in the same shop (excluding current rank)
                if (request.Threshold.HasValue && await _rankRepo.IsThresholdExistsAsync(request.Threshold.Value, request.ShopId, existing.RankId))
                {
                    return new ApiResponse<RankResponse>
                    {
                        Success = false,
                        Message = "Threshold value already exists in this shop",
                        Data = null
                    };
                }

                // Validation 3.5: Handle null threshold logic for update
                if (!request.Threshold.HasValue)
                {
                    // If current rank is not the null threshold rank and we're trying to set it to null
                    if (existing.Threshold.HasValue)
                    {
                        var existingNullThresholdRank = await _rankRepo.GetRankWithNullThresholdAsync(request.ShopId, existing.RankId);
                        if (existingNullThresholdRank != null)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot update rank to have null threshold: there's already a rank with null threshold (ID: {existingNullThresholdRank.RankId}). Only one rank per shop can have null threshold.",
                                Data = null
                            };
                        }
                        
                        // If no existing null threshold rank, check if this rank should have the highest benefit
                        if (request.Benefit.HasValue)
                        {
                            var maxBenefit = await _rankRepo.GetMaxBenefitInShopAsync(request.ShopId, existing.RankId);
                            if (maxBenefit.HasValue && request.Benefit.Value < maxBenefit.Value)
                            {
                                return new ApiResponse<RankResponse>
                                {
                                    Success = false,
                                    Message = $"Cannot update rank to have null threshold: the rank's benefit ({request.Benefit.Value}) is not the highest. Only the rank with the highest benefit can have null threshold.",
                                    Data = null
                                };
                            }
                        }
                    }
                    // If current rank already has null threshold, allow update but check benefit consistency
                    else if (request.Benefit.HasValue)
                    {
                        var maxBenefit = await _rankRepo.GetMaxBenefitInShopAsync(request.ShopId, existing.RankId);
                        if (maxBenefit.HasValue && request.Benefit.Value < maxBenefit.Value)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot update null threshold rank: the rank's benefit ({request.Benefit.Value}) must be the highest. Only the rank with the highest benefit can have null threshold.",
                                Data = null
                            };
                        }
                    }
                }
                else
                {
                    // If updating from null threshold to non-null threshold, check if another rank should become null threshold
                    if (!existing.Threshold.HasValue && request.Benefit.HasValue)
                    {
                        var maxBenefit = await _rankRepo.GetMaxBenefitInShopAsync(request.ShopId, existing.RankId);
                        if (maxBenefit.HasValue && request.Benefit.Value < maxBenefit.Value)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot update null threshold rank to have threshold: this rank currently has the highest benefit and must keep null threshold. Another rank with benefit {maxBenefit.Value} should have null threshold instead.",
                                Data = null
                            };
                        }
                    }
                    
                    // If updating to non-null threshold, check if this rank should have the highest benefit
                    if (request.Benefit.HasValue)
                    {
                        var existingNullThresholdRank = await _rankRepo.GetRankWithNullThresholdAsync(request.ShopId, existing.RankId);
                        if (existingNullThresholdRank != null && existingNullThresholdRank.Benefit.HasValue && request.Benefit.Value > existingNullThresholdRank.Benefit.Value)
                        {
                            return new ApiResponse<RankResponse>
                            {
                                Success = false,
                                Message = $"Cannot update rank: the updated rank's benefit ({request.Benefit.Value}) is higher than the existing null threshold rank's benefit ({existingNullThresholdRank.Benefit.Value}). The rank with highest benefit must have null threshold.",
                                Data = null
                            };
                        }
                    }
                }

                // Validation 4: Check threshold-benefit consistency
                if (request.Threshold.HasValue && request.Benefit.HasValue)
                {
                    var existingRanks = await _rankRepo.GetRanksByShopIdAsync(request.ShopId, existing.RankId);
                    
                    // Check if updated rank's benefit is less than any rank with lower threshold
                    var conflictingRank = existingRanks.FirstOrDefault(r => 
                        r.Threshold < request.Threshold.Value && 
                        r.Benefit.HasValue && 
                        r.Benefit.Value > request.Benefit.Value);
                    
                    if (conflictingRank != null)
                    {
                        return new ApiResponse<RankResponse>
                        {
                            Success = false,
                            Message = $"Benefit value ({request.Benefit.Value}) cannot be less than benefit ({conflictingRank.Benefit.Value}) of rank with lower threshold ({conflictingRank.Threshold.Value})",
                            Data = null
                        };
                    }

                    // Check if any existing rank with higher threshold has benefit less than updated rank's benefit
                    var higherThresholdRank = existingRanks.FirstOrDefault(r => 
                        r.Threshold > request.Threshold.Value && 
                        r.Benefit.HasValue && 
                        r.Benefit.Value < request.Benefit.Value);
                    
                    if (higherThresholdRank != null)
                    {
                        return new ApiResponse<RankResponse>
                        {
                            Success = false,
                            Message = $"Cannot update rank: existing rank with higher threshold ({higherThresholdRank.Threshold.Value}) has lower benefit ({higherThresholdRank.Benefit.Value}) than this rank's benefit ({request.Benefit.Value})",
                            Data = null
                        };
                    }
                }

                _mapper.Map(request, existing);

                var affected = await _rankRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<RankResponse>(existing);
                    return new ApiResponse<RankResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<RankResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<RankResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
