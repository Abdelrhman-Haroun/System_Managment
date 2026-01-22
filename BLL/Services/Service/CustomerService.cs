using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Customer;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CustomerService> logger)
        {
            _unit = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(string searchTerm = null)
        {
            try
            {
                var customers = await _unit.Customer.GetAllAsync(c => !c.IsDeleted);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    customers = customers.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        (c.Phone != null && c.Phone.Contains(term))
                    );
                }

                return customers.OrderBy(c => c.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(Expression<Func<Customer, bool>> filter = null, string includes = null)
        {
            return await _unit.Customer.GetAllAsync(filter, includes);
        }
        public async Task<Customer> GetByIdAsync(int id)
        {
            try
            {
                return await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by ID: {Id}", id);
                throw;
            }
        }

        public async Task<Customer> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                return await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == name.Trim().ToLower() && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by name: {Name}", name);
                throw;
            }
        }

        public async Task<Customer> CreateAsync(CreateCustomerVM model)
        {
            try
            {
                // Check for duplicate name
                var existingCustomer = await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted
                );

                if (existingCustomer != null)
                {
                    throw new InvalidOperationException("يوجد عميل بنفس الاسم");
                }

                // Map ViewModel to Entity
                var customer = _mapper.Map<Customer>(model);
                customer.Balance ??= 0;
                // Add to database
                _unit.Customer.Add(customer);
                await _unit.CompleteAsync();

                _logger.LogInformation("Customer created successfully: {Name}", customer.Name);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {Name}", model?.Name);
                throw;
            }
        }

        public async Task<Customer> UpdateAsync(EditCustomerVM model)
        {
            try
            {
                // Get existing customer
                var customer = await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Id == model.Id && !x.IsDeleted
                );

                if (customer == null)
                {
                    throw new InvalidOperationException("العميل غير موجود");
                }

                // Check for duplicate name (excluding current customer)
                var duplicateCustomer = await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower()
                         && x.Id != model.Id
                         && !x.IsDeleted
                );

                if (duplicateCustomer != null)
                {
                    throw new InvalidOperationException("هذا الاسم مستخدم من عميل آخر");
                }

                // Update properties
                customer.Name = model.Name.Trim();
                customer.Phone = model.Phone?.Trim();
                customer.Address = model.Address?.Trim();
                customer.UpdatedAt = DateTime.UtcNow;

                // Save changes
                _unit.Customer.Update(customer);
                await _unit.CompleteAsync();

                _logger.LogInformation("Customer updated successfully: {Id}", customer.Id);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {Id}", model?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var customer = await _unit.Customer.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );

                if (customer == null)
                {
                    return false;
                }

                // Soft delete
                customer.IsDeleted = true;

                _unit.Customer.Update(customer);
                await _unit.CompleteAsync();

                _logger.LogInformation("Customer deleted successfully: {Id}", customer.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {Id}", id);
                throw;
            }
        }
    }
}