using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IAuthenticationService authenticationService)
        {
            _httpClient = httpClientFactory.CreateClient("BePlatform");
            _configuration = configuration;
            _authenticationService = authenticationService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var localResponse = await _authenticationService.CheckLogin(request);

            if (localResponse == null || !localResponse.Success || localResponse.Data == null)
            {
                return Unauthorized(localResponse);
            }

            var bePlatformUrl = _configuration.GetValue<string>("BePlatformURL:Url");
            var loginEndpoint = $"{bePlatformUrl}/api/authentication/validate-tenant-login";

            var requestPayload = new
            {
                UserId = localResponse.Data?.UserId,
                Username = localResponse.Data?.Username,
                Role = localResponse.Data?.Role,
                ShopId = localResponse.Data?.ShopId
            };


            // Gọi API validate shop login bên BE platform
            var json = System.Text.Json.JsonSerializer.Serialize(requestPayload);
            Console.WriteLine("Sending JSON to BE platform: " + json);
            var response = await _httpClient.PostAsJsonAsync(loginEndpoint, requestPayload);
            


            // 4. Deserialize JSON trả về từ BE platform
            var bePlatformResult = await response.Content.ReadFromJsonAsync<ApiResponse<ValidateShopResponse>>();

            if (bePlatformResult == null || !bePlatformResult.Success || bePlatformResult.Data == null)
            {
                return Unauthorized(bePlatformResult);
            }
            localResponse.Data.AccessToken = bePlatformResult.Data.AccessToken;
            return Ok(localResponse);
        }
    }
}
