using DAL.Models;
using System.Linq.Expressions;

namespace DAL.IRepository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
    
        public Task<ICollection<Product>> GetDefaultProducts(Expression<Func<Product, bool>> filter = null);
        public Task<ICollection<Product>> GetAllProductsAsync(Expression<Func<Product, bool>> filter = null);
        public Task<Product> GetProductById(Expression<Func<Product, bool>> filter = null);
        public Task<ICollection<Product>> GetProductsByCategory(Expression<Func<Product, bool>> filter = null);
        public Task<Product> CreateProductAsync(Product product);
        public Task<Product> UpdateProductAsync(Product product);
        public Task DeleteProduct(int id);
    }
}
