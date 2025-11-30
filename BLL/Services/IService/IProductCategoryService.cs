using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IStoreService
    {

            Task<IEnumerable<Store>> GetAllAsync(Expression<Func<Store, bool>> filter = null);
            Task<Store> GetByIdAsync(int id);
            Task<Store> GetByNameAsync(string name);
            Task<Store> CreateAsync(Store Store);
            Task<Store> UpdateAsync(Store Store);
            Task DeleteAsync(int id);
    }
    
}