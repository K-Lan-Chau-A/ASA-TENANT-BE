using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/chat-messages")]
    [ApiController]
    public class ChatMessageController : ControllerBase
    {
        private readonly IChatMessageService _chatMessageService;
        public ChatMessageController(IChatMessageService chatMessageService)
        {
            _chatMessageService = chatMessageService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetFiltered([FromQuery] ChatMessageGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _chatMessageService.GetFilteredChatMessageAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<ChatMessageResponse>> Create([FromBody] ChatMessageRequest request)
        {
            try
            {
                var result = await _chatMessageService.CreateAsync(request);
                if (!result.Success || result.Data == null)
                {
                    return BadRequest(result);
                }
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ChatMessageResponse>> Update(long id, [FromBody] ChatMessageRequest request)
        {
            try
            {
                var result = await _chatMessageService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "ChatMessage not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            try
            {
                var result = await _chatMessageService.DeleteAsync(id);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "ChatMessage not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
