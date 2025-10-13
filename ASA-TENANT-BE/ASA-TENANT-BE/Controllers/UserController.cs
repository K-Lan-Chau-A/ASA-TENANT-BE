using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetFiltered([FromQuery] UserGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetFilteredUsersAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("create-staff")]
        public async Task<ActionResult<UserResponse>> CreateStaff([FromForm] UserCreateRequest request)
        {
            var result = await _userService.CreateStaffAsync(request);
            return Ok(result);
        }
        //[HttpPost("create-admin")]
        ////public async Task<ActionResult<UserAdminResponse>> CreateAdmin([FromBody] UserAdminCreateRequest request)
        ////{
        ////    var result = await _userService.CreateAdminAsync(request);
        ////    return Ok(result);
        ////}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> Update(long id, [FromForm] UserUpdateRequest request)
        {
            var result = await _userService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _userService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
