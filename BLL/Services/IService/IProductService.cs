using BLL.ViewModels.Product;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null, string includes = null);
        Task<IEnumerable<Product>> GetAllAsync(string searchTerm = null, string includes = null);
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetByNameAsync(string name);
        Task<Product> CreateAsync(CreateProductVM model);
        Task<Product> UpdateAsync(EditProductVM model);
        Task<bool> DeleteAsync(int id);
    }
    
}