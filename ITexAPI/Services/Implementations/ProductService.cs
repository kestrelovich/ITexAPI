using ITexAPI.Data.Repositories.Interfaces;
using ITexAPI.Models.DTOs;
using ITexAPI.Models.DTOs.Common;
using ITexAPI.Models.Entities;
using ITexAPI.Services.Interfaces;
using AutoMapper;

namespace ITexAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _productRepo.GetProductWithImagesAsync(id);
            if (product == null) throw new Exception("Product not found");
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductSummaryDto>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductSummaryDto>>(products);
        }

        public async Task<PaginatedResponse<ProductSummaryDto>> GetPaginatedAsync(PaginationParams paginationParams)
        {
            var paged = await _productRepo.GetProductsWithFiltersAsync(paginationParams);
            return new PaginatedResponse<ProductSummaryDto>
            {
                Data = _mapper.Map<List<ProductSummaryDto>>(paged.Data),
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                HasPrevious = paged.HasPrevious,
                HasNext = paged.HasNext
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            var created = await _productRepo.AddAsync(entity);
            return _mapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
        {
            var existing = await _productRepo.GetByIdAsync(id);
            if (existing == null) throw new Exception("Product not found");
            _mapper.Map(dto, existing);
            var updated = await _productRepo.UpdateAsync(existing);
            return _mapper.Map<ProductDto>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            await _productRepo.DeleteAsync(id);
        }
    }
}
