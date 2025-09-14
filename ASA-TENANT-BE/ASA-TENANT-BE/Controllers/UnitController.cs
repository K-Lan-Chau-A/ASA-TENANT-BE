using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/units")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _unitService;
        public UnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitResponse>>> GetFiltered([FromQuery] UnitGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _unitService.GetFilteredUnitsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<UnitResponse>> Create([FromBody] UnitRequest request)
        {
            var result = await _unitService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<UnitResponse>> Update(long id, [FromBody] UnitRequest request)
        {
            var result = await _unitService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _unitService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
