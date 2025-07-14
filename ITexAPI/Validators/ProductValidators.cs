using FluentValidation;
using ITexAPI.Models.DTOs;

namespace ITexAPI.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .Length(3, 50).WithMessage("SKU must be between 3 and 50 characters")
                .Matches("^[A-Za-z0-9-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens, and underscores");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(1000000).WithMessage("Price cannot exceed 1,000,000");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            // Textile-specific validations
            RuleFor(x => x.FabricType)
                .MaximumLength(50).WithMessage("Fabric type cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FabricType));

            RuleFor(x => x.Color)
                .MaximumLength(30).WithMessage("Color cannot exceed 30 characters")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.Pattern)
                .MaximumLength(50).WithMessage("Pattern cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Pattern));

            RuleFor(x => x.Size)
                .MaximumLength(20).WithMessage("Size cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Size));

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0")
                .LessThan(1000).WithMessage("Weight cannot exceed 1000")
                .When(x => x.Weight.HasValue);

            RuleFor(x => x.CareInstructions)
                .MaximumLength(500).WithMessage("Care instructions cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.CareInstructions));

            RuleFor(x => x.Composition)
                .MaximumLength(200).WithMessage("Composition cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Composition));

            RuleFor(x => x.Season)
                .MaximumLength(20).WithMessage("Season cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Season));
        }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(1000000).WithMessage("Price cannot exceed 1,000,000");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            // Textile-specific validations
            RuleFor(x => x.FabricType)
                .MaximumLength(50).WithMessage("Fabric type cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FabricType));

            RuleFor(x => x.Color)
                .MaximumLength(30).WithMessage("Color cannot exceed 30 characters")
                .When(x => !string.IsNullOrEmpty(x.Color));

            RuleFor(x => x.Pattern)
                .MaximumLength(50).WithMessage("Pattern cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Pattern));

            RuleFor(x => x.Size)
                .MaximumLength(20).WithMessage("Size cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Size));

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0")
                .LessThan(1000).WithMessage("Weight cannot exceed 1000")
                .When(x => x.Weight.HasValue);

            RuleFor(x => x.CareInstructions)
                .MaximumLength(500).WithMessage("Care instructions cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.CareInstructions));

            RuleFor(x => x.Composition)
                .MaximumLength(200).WithMessage("Composition cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Composition));

            RuleFor(x => x.Season)
                .MaximumLength(20).WithMessage("Season cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Season));
        }
    }
}