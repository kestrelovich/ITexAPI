using ITexAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITexAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Cotton Fabrics", Description = "High-quality cotton textiles" },
                    new Category { Name = "Silk Fabrics", Description = "Luxurious silk materials" },
                    new Category { Name = "Wool Fabrics", Description = "Warm and durable wool textiles" },
                    new Category { Name = "Synthetic Fabrics", Description = "Modern synthetic materials" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Add subcategories
                var cottonId = categories.First(c => c.Name == "Cotton Fabrics").Id;
                var silkId = categories.First(c => c.Name == "Silk Fabrics").Id;

                var subcategories = new List<Category>
                {
                    new Category { Name = "Organic Cotton", Description = "Eco-friendly organic cotton", ParentId = cottonId },
                    new Category { Name = "Denim", Description = "Durable denim fabrics", ParentId = cottonId },
                    new Category { Name = "Pure Silk", Description = "100% pure silk fabrics", ParentId = silkId },
                    new Category { Name = "Silk Blends", Description = "Silk mixed with other fibers", ParentId = silkId }
                };

                await context.Categories.AddRangeAsync(subcategories);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!await userManager.Users.AnyAsync())
            {
                var adminUser = new User
                {
                    UserName = "admin@itex.com",
                    Email = "admin@itex.com",
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
            }

            // Seed Sample Products
            if (!context.Products.Any())
            {
                var cottonCategory = context.Categories.First(c => c.Name == "Cotton Fabrics");
                var silkCategory = context.Categories.First(c => c.Name == "Silk Fabrics");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Premium Cotton Canvas",
                        Description = "Heavy-duty cotton canvas perfect for upholstery and crafts",
                        SKU = "COT-001",
                        Price = 15.99m,
                        StockQuantity = 100,
                        CategoryId = cottonCategory.Id,
                        FabricType = "Cotton",
                        Color = "Natural",
                        Weight = 0.8m,
                        Composition = "100% Cotton",
                        CareInstructions = "Machine wash cold, tumble dry low"
                    },
                    new Product
                    {
                        Name = "Luxury Silk Charmeuse",
                        Description = "Smooth and lustrous silk charmeuse for elegant garments",
                        SKU = "SLK-001",
                        Price = 45.99m,
                        StockQuantity = 50,
                        CategoryId = silkCategory.Id,
                        FabricType = "Silk",
                        Color = "Ivory",
                        Weight = 0.2m,
                        Composition = "100% Mulberry Silk",
                        CareInstructions = "Dry clean only"
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}