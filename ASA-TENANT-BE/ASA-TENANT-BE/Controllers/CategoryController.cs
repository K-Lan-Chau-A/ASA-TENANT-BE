using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASA_TENANT_BE.CustomAttribute;
using System.Diagnostics;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    [RequireFeature(8)] // Quản lí category
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetFiltered([FromQuery] CategoryGetRequest requestDto, [FromQuery] int page = 1,[FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _categoryService.GetFilteredCategoriesAsync(requestDto, page,pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryRequest request)
        {
            var result = await _categoryService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryResponse>> Update(long id ,[FromBody] CategoryRequest request)
        {
            var result = await _categoryService.UpdateAsync(id,request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Admin only
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
