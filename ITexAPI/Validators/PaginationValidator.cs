using FluentValidation;
using ITexAPI.Models.DTOs.Common;

namespace ITexAPI.Validators
{
    public class PaginationParamsValidator : AbstractValidator<PaginationParams>
    {
        public PaginationParamsValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.SortBy)
                .MaximumLength(50).WithMessage("Sort by field cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortOrder)
                .Must(x => x == "asc" || x == "desc").WithMessage("Sort order must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.SortOrder));
        }
    }
}