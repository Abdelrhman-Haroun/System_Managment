using DAL.Models;
using System.Linq.Expressions;

namespace DAL.Repositories.IRepository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        public Task<Product> GetByIdContainsAsync(int Id, string? includeWord = null);
    }
}
