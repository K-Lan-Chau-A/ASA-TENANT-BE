using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASA_TENANT_BE.CustomAttribute;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/products")]
    [ApiController]
    [Authorize]
    [RequireFeature(5)] // Quản lí sản phẩm
    public class ProductController : ControllerBase
    {
        private readonly IProductService  _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult> GetFiltered([FromQuery] ProductGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _productService.GetFilteredProductsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create([FromForm] ProductRequest request)
        {
            // Coalesce duplicated UnitsJson form fields into a single JSON array
            if (Request.HasFormContentType && Request.Form.TryGetValue("UnitsJson", out var unitsValues) && unitsValues.Count > 1)
            {
                // If multiple values provided, wrap them into an array string
                // Values are expected to be individual JSON objects
                var joined = "[" + string.Join(',', unitsValues) + "]";
                request.UnitsJson = joined;
            }
            try
            {
                var result = await _productService.CreateAsync(request);
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
        public async Task<ActionResult<ProductResponse>> Update(long id, [FromForm] ProductUpdateRequest request)
        {
            if (Request.HasFormContentType && Request.Form.TryGetValue("UnitsJson", out var unitsValues) && unitsValues.Count > 1)
            {
                var joined = "[" + string.Join(',', unitsValues) + "]";
                request.UnitsJson = joined;
            }
            try
            {
                var result = await _productService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "Product not found", StringComparison.OrdinalIgnoreCase))
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
        [Authorize(Roles = "1")] // Admin only
        public async Task<ActionResult<bool>> Delete(long id, long shopid)
        {
            try
            {
                var result = await _productService.DeleteAsync(id,shopid);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "Product not found", StringComparison.OrdinalIgnoreCase))
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
