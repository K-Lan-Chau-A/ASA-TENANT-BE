using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/product-units")]
    [ApiController]
    public class ProductUnitController : ControllerBase
    {
        private readonly IProductUnitService _productUnitService;
        public ProductUnitController(IProductUnitService productUnitService)
        {
            _productUnitService = productUnitService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductUnit>>> GetFiltered([FromQuery] ProductUnitGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _productUnitService.GetFilteredProductUnitsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<ProductUnitResponse>> Create([FromBody] ProductUnitRequest request)
        {
            var result = await _productUnitService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryResponse>> Update(long id, [FromBody] ProductUnitRequest request)
        {
            var result = await _productUnitService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _productUnitService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
