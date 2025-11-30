using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface ISupplierService
    {

            Task<IEnumerable<Supplier>> GetAllAsync(Expression<Func<Supplier, bool>> filter = null);
            Task<Supplier> GetByIdAsync(int id);
            Task<Supplier> GetByNameAsync(string name);
            Task<Supplier> CreateAsync(Supplier supplier);
            Task<Supplier> UpdateAsync(Supplier supplier);
            Task DeleteAsync(int id);
    }
    
}