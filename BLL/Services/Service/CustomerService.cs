
using BLL.Services.IService;
using DAL.Repositories.IRepository;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unit;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unit = unitOfWork;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>> filter = null)
        {
            return await _unit.Customer.GetAllAsync(filter);
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _unit.Customer.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        } 
        public async Task<Customer> GetByNameAsync(string name)
        {
            return await _unit.Customer.GetFirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<Customer> CreateAsync(Customer Customer)
        {
            _unit.Customer.Add(Customer);
            _unit.Complete();
            return Customer;
        }

        public async Task<Customer> UpdateAsync(Customer Customer)
        {
            _unit.Customer.Update(Customer); 
            _unit.Complete();
            return Customer;
        }

        public async Task DeleteAsync(int id)
        {
            var Customer = await _unit.Customer.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (Customer != null)
            {
                //_unit.Customer.Remove(Customer);
                Customer.IsDeleted = true;
                _unit.Complete();
            }
        }
    }
}
