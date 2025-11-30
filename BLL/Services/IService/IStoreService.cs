using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IProductCategoryService
    {

            Task<IEnumerable<ProductCategory>> GetAllAsync(Expression<Func<ProductCategory, bool>> filter = null);
            Task<ProductCategory> GetByIdAsync(int id);
            Task<ProductCategory> GetByNameAsync(string name);
            Task<ProductCategory> CreateAsync(ProductCategory ProductCategory);
            Task<ProductCategory> UpdateAsync(ProductCategory ProductCategory);
            Task DeleteAsync(int id);
    }
    
}