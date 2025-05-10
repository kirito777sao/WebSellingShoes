using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebSellingShoes.Repository
{
    public class SeedData
    {
        public static void Seed(DataContext _context)
        {
            // Seed data here
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                BrandModel nike = new BrandModel { Name = "Nike", Slug = "nike", Description = "Nike is the best", Status = 1 };
                BrandModel adidas = new BrandModel { Name = "Adidas", Slug = "adidas", Description = "Adidas is the best", Status = 1 };
                BrandModel puma = new BrandModel { Name = "Puma", Slug = "puma", Description = "Puma is the best", Status = 1 };
                BrandModel nb = new BrandModel { Name = "New Balance", Slug = "nb", Description = "New Balance is the best", Status = 1 };

                CategoryModel sport = new CategoryModel { Name = "Sport", Slug = "sport", Description = "Shoes to participate in sports activities", Status = 1 };
                CategoryModel male = new CategoryModel { Name = "Male", Slug = "male", Description = "Shoes for men", Status = 1 };
                CategoryModel female = new CategoryModel { Name = "FeMale", Slug = "female", Description = "Shoes for women", Status = 1 };
                CategoryModel sale = new CategoryModel { Name = "Sale", Slug = "sale", Description = "Shoes are discounted", Status = 1 };
                CategoryModel feature = new CategoryModel { Name = "Feature", Slug = "feature", Description = "Shoes are currently selling and the most popular", Status = 1 };

                _context.Products.AddRange(
                   new ProductModel { Name = "Nike Air Force One", Slug = "nike-air-force-1", Description = "Nike Air Force One is the best", Price = 2000000, Image = "af1.jpg", Category = feature, Brand = nike },
                   new ProductModel { Name = "Adidas Superstar", Slug = "adidas-superstar", Description = "Adidas Superstar is the best", Price = 1500000, Image = "adidasss.jpg", Category = feature, Brand = adidas },
                   new ProductModel { Name = "Puma RS-X", Slug = "puma-rs-x", Description = "Puma RS-X is the best", Price = 1800000, Image = "pumarsx.jpg", Category = feature, Brand = puma },
                   new ProductModel { Name = "New Balance 997", Slug = "new-balance-997", Description = "New Balance 997 is the best", Price = 2500000, Image = "nb997.jpg", Category = feature, Brand = nb }
                );
                _context.SaveChanges();
            }
        }
    }
}
