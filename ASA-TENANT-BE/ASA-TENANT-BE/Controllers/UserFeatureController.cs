using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/userfeature")]
    [ApiController]
    public class UserFeatureController : ControllerBase
    {
        private readonly IUserFeatureService _userFeatureService;

        public UserFeatureController(IUserFeatureService userFeatureService)
        {
            _userFeatureService = userFeatureService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserFeatureResponse>>> GetFiltered([FromQuery] UserFeatureGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userFeatureService.GetFilteredUsersFeatureAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<List<UserFeatureResponse>>> Create([FromBody] UserFeatureRequest request)
        {
            var result = await _userFeatureService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut()]
        public async Task<ActionResult<List<UserFeatureResponse>>> Update( [FromBody] UserFeatureUpdateRequest request)
        {
            var result = await _userFeatureService.UpdateAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _userFeatureService.DeleteAsync(id);
            return Ok(result);
        }
    }
}

