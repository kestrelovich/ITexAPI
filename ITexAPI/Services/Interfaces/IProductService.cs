using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<IEnumerable<ProductSummaryDto>> GetAllAsync();
        Task<PaginatedResponse<ProductSummaryDto>> GetPaginatedAsync(PaginationParams paginationParams);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
        Task DeleteAsync(int id);
    }
}