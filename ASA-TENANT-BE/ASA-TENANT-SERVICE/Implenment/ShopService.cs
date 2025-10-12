using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Google.Apis.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ShopService : IShopService
    {
        private readonly ShopRepo _shopRepo;
        private readonly ShopSubscriptionRepo _shopSubscriptionRepo;
        private readonly UserRepo _userRepo;
        private readonly UserFeatureRepo _userFeatureRepo;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ShopService(ShopRepo shopRepo,
                           ShopSubscriptionRepo shopSubscriptionRepo,
                           UserRepo userRepo,
                           UserFeatureRepo userFeatureRepo,
                           IMapper mapper,
                           IConfiguration configuration,
                           System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _shopRepo = shopRepo;
            _shopSubscriptionRepo = shopSubscriptionRepo;
            _userRepo = userRepo;
            _userFeatureRepo = userFeatureRepo;
            _mapper = mapper;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("BEPlatformUrl");
        }

        private static string GenerateRandomPassword(int length = 8)
        {
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = allowedChars[bytes[i] % allowedChars.Length];
            }

            return new string(result);
        }

        public async Task<ApiResponse<ShopResponse>> CreateAsync(ShopRequest request)
        {
            try
            {
                var existingUserByUsername = await _userRepo.GetUserByUsername(request.Username);
                if (existingUserByUsername != null)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Username already exists",
                        Data = null
                    };
                }

                var entity = _mapper.Map<Shop>(request);
                entity.CreatedAt = DateTime.UtcNow;

                var affected = await _shopRepo.CreateAsync(entity);

                if (affected <= 0)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Create failed",
                        Data = null
                    };
                }

                // Gọi API nền tảng để lấy sản phẩm và tính năng
                var patformApiUrl = _configuration.GetValue<string>("BEPlatformUrl:Url");
                var url = $"{patformApiUrl}/api/products?productId={request.ProductId}&page=1&pageSize=10";

                using var platformResponse = await _httpClient.GetAsync(url);

                if (!platformResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = $"Platform API error: {(int)platformResponse.StatusCode}",
                        Data = null
                    };
                }

                using var contentStream = await platformResponse.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(contentStream);
                var root = jsonDoc.RootElement;
                var items = root.GetProperty("items");
                if (items.GetArrayLength() == 0)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Product not found on platform",
                        Data = null
                    };
                }

                var productElement = items[0];
                var featureElements = productElement.GetProperty("features");

                // Tạo đăng ký cửa hàng để dùng thử trong 7 ngày
                var now = DateTime.UtcNow;
                var subscription = new ShopSubscription
                {
                    ShopId = entity.ShopId,
                    PlatformProductId = request.ProductId,
                    StartDate = now,
                    EndDate = now.AddDays(7),
                    Status = 1,
                    CreatedAt = now
                };

                await _shopSubscriptionRepo.CreateAsync(subscription);

                // Tạo người dùng quản trị với tên người dùng và mật khẩu ngẫu nhiên
                var plainPassword = GenerateRandomPassword(8);
                var adminUser = new User
                {
                    Username = request.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                    Status = 1,
                    ShopId = entity.ShopId,
                    Role = 1,
                    CreatedAt = now
                };
                await _userRepo.CreateAsync(adminUser);

                // Tạo các tính năng người dùng từ các tính năng nền tảng
                var userFeatures = new List<UserFeature>();
                foreach (var feature in featureElements.EnumerateArray())
                {
                    var featureId = feature.GetProperty("featureId").GetInt64();
                    var featureName = feature.GetProperty("featureName").GetString();
                    userFeatures.Add(new UserFeature
                    {
                        UserId = adminUser.UserId,
                        FeatureId = featureId,
                        FeatureName = featureName,
                        IsEnabled = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                if (userFeatures.Count > 0)
                {
                    await _userFeatureRepo.AddRangeAsync(userFeatures);
                    await _userFeatureRepo.SaveChangesAsync();
                }

                var response = _mapper.Map<ShopResponse>(entity);
                response.CreatedAdminUsername = request.Username;
                response.CreatedAdminPassword = plainPassword;
                return new ApiResponse<ShopResponse>
                {
                    Success = true,
                    Message = "Create successfully",
                    Data = response
                };

            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
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
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = false
                    };

                var affected = await _shopRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ShopResponse>> GetFilteredShopsAsync(ShopGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Shop>(Filter);
            var query = _shopRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ShopResponse>
            {
                Items = _mapper.Map<IEnumerable<ShopResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ShopResponse>> UpdateAsync(long id, ShopRequest request)
        {
            try
            {
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _shopRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopResponse>(existing);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> UpdateSepayApiKeyAsync(long id, string apiKey)
        {
            try
            {
                // Sử dụng method riêng chỉ cập nhật SepayApiKey
                var affected = await _shopRepo.UpdateSepayApiKeyAsync(id, apiKey);
                
                if (affected > 0)
                {
                    // Lấy lại shop sau khi cập nhật
                    var updatedShop = await _shopRepo.GetByIdAsync(id);
                    var response = _mapper.Map<ShopResponse>(updatedShop);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Sepay API key updated successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Shop not found or failed to update Sepay API key",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> TestSepayApiKeyAsync(string apiKey)
        {
            try
            {
                var shops = await _shopRepo.GetAllAsync();
                var shop = shops.FirstOrDefault(s => s.SepayApiKey == apiKey);

                if (shop != null)
                {
                    var response = _mapper.Map<ShopResponse>(shop);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "API key is valid",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Invalid API key",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> UpdateBankInfoAsync(long id, string bankName, string bankCode, string bankNum)
        {
            try
            {
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                existing.BankName = bankName;
                existing.BankCode = bankCode;
                existing.BankNum = bankNum;

                var affected = await _shopRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopResponse>(existing);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Bank info updated successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Failed to update bank info",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
