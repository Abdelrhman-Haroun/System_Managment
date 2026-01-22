using BLL.ViewModels.Account;
using BLL.ViewModels.Supplier;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync(string searchTerm = null);
        Task<IEnumerable<Supplier>> GetAllAsync(Expression<Func<Supplier, bool>> filter = null, string includes = null);
        Task<Supplier> GetByIdAsync(int id);
        Task<Supplier> GetByNameAsync(string name);
        Task<Supplier> CreateAsync(CreateSupplierVM model);
        Task<Supplier> UpdateAsync(EditSupplierVM model);
        Task<bool> DeleteAsync(int id);
    }

}