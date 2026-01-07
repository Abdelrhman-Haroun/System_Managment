using BLL.ViewModels.ProductCategory;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync(string searchTerm = null);
        Task<IEnumerable<ProductCategory>> GetAllAsync(Expression<Func<ProductCategory, bool>> filter = null, string includes = null);
        Task<ProductCategory> GetByIdAsync(int id);
        Task<ProductCategory> GetByNameAsync(string name);
        Task<ProductCategory> CreateAsync(CreateProductCategoryVM model);
        Task<ProductCategory> UpdateAsync(EditProductCategoryVM model);
        Task<bool> DeleteAsync(int id);
    }
    
}