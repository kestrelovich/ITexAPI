using Microsoft.AspNetCore.Mvc;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Services.Interfaces;

namespace ITexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSummaryDto>>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductSummaryDto>>.SuccessResponse(products));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}
