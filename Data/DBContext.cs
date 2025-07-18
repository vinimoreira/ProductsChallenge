using Microsoft.EntityFrameworkCore;
using Products.Entities;

namespace Products.Data
{
    public class ProductDBContext: DbContext
    {
        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
    }
}
