using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetFiltered([FromQuery] TransactionGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _transactionService.GetFilteredTransactionsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponse>> Create([FromBody] TransactionRequest request)
        {
            try
            {
                var result = await _transactionService.CreateAsync(request);
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
        public async Task<ActionResult<TransactionResponse>> Update(long id, [FromBody] TransactionRequest request)
        {
            try
            {
                var result = await _transactionService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "Transaction not found", StringComparison.OrdinalIgnoreCase))
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
                var result = await _transactionService.DeleteAsync(id);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "Transaction not found", StringComparison.OrdinalIgnoreCase))
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


