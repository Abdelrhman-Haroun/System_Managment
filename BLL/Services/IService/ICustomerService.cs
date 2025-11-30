using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.IService
{
    public interface ICustomerService
    {
            Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>> filter = null);
            Task<Customer> GetByIdAsync(int id);
            Task<Customer> GetByNameAsync(string name);
            Task<Customer> CreateAsync(Customer Customer);
            Task<Customer> UpdateAsync(Customer Customer);
            Task DeleteAsync(int id);
    }
    
}