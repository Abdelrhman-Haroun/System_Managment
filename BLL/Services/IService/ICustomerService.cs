using BLL.ViewModels.Account;
using BLL.ViewModels.Customer;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync(string searchTerm = null);
        Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>> filter = null, string includes = null);
        Task<Customer> GetByIdAsync(int id);
        Task<Customer> GetByNameAsync(string name);
        Task<Customer> CreateAsync(CreateCustomerVM model);
        Task<Customer> UpdateAsync(EditCustomerVM model);
        Task<bool> DeleteAsync(int id);
    }
    
}