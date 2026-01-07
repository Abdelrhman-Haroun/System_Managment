using BLL.ViewModels.Store;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface IStoreService
    {
        Task<IEnumerable<Store>> GetAllAsync(string searchTerm = null);
        Task<IEnumerable<Store>> GetAllAsync(Expression<Func<Store, bool>> filter = null, string includes = null);
        Task<Store> GetByIdAsync(int id);
        Task<Store> GetByNameAsync(string name);
        Task<Store> CreateAsync(CreateStoreVM model);
        Task<Store> UpdateAsync(EditStoreVM model);
        Task<bool> DeleteAsync(int id);
    }
    
}