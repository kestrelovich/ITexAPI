using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSummaryDto>>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductSummaryDto>>.SuccessResponse(products));
        }

        [HttpGet("paginated")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductSummaryDto>>>> GetPaginated([FromQuery] PaginationParams paginationParams)
        {
            var products = await _productService.GetPaginatedAsync(paginationParams);
            return Ok(ApiResponse<PaginatedResponse<ProductSummaryDto>>.SuccessResponse(products));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("filters")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> GetFilters()
        {
            var products = await _productService.GetAllAsync();

            var filters = new
            {
                FabricTypes = products.Where(p => !string.IsNullOrEmpty(p.FabricType))
                    .Select(p => p.FabricType).Distinct().OrderBy(x => x).ToList(),
                Colors = products.Where(p => !string.IsNullOrEmpty(p.Color))
                    .Select(p => p.Color).Distinct().OrderBy(x => x).ToList(),
                Sizes = products.Where(p => !string.IsNullOrEmpty(p.Size))
                    .Select(p => p.Size).Distinct().OrderBy(x => x).ToList(),
                PriceRange = new
                {
                    Min = products.Any() ? products.Min(p => p.Price) : 0,
                    Max = products.Any() ? products.Max(p => p.Price) : 0
                }
            };

            return Ok(ApiResponse<object>.SuccessResponse(filters));
        }
    }
}
