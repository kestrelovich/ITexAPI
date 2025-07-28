using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PaginatedResponse<ProductSummaryDto>>> GetProductsAsync(PaginationParams paginationParams, int? categoryId = null);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<ProductDto>> GetProductBySKUAsync(string sku);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<ApiResponse<bool>> DeleteProductAsync(int id);
        Task<ApiResponse<bool>> UpdateStockAsync(int productId, int newStock);
    }
}