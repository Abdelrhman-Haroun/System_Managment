using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IProductService
    {
            Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null, string includes = null);
            Task<Product> GetByIdAsync(int id);
            Task<Product> GetByNameAsync(string name);
            Task<Product> CreateAsync(Product Product);
            Task<Product> UpdateAsync(Product Product);
            Task DeleteAsync(int id);
    }
    
}