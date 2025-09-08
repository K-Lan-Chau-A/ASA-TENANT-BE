using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/prompts")]
    [ApiController]
    public class PromptController : ControllerBase
    {
        private readonly IPromptService _promptService;
        public PromptController(IPromptService promptService)
        {
            _promptService = promptService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prompt>>> GetFiltered([FromQuery] PromptGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _promptService.GetFilteredPromptsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<PromptResponse>> Create([FromBody] PromptRequest request)
        {
            var result = await _promptService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<PromptResponse>> Update(long id, [FromBody] PromptRequest request)
        {
            var result = await _promptService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _promptService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
