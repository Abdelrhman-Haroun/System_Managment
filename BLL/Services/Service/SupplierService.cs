using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Supplier;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SupplierService> logger)
        {
            _unit = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(string searchTerm = null)
        {
            try
            {
                var Suppliers = await _unit.Supplier.GetAllAsync(c => !c.IsDeleted);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    Suppliers = Suppliers.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        (c.Phone != null && c.Phone.Contains(term))
                    );
                }

                return Suppliers.OrderBy(c => c.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Suppliers");
                throw;
            }
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(Expression<Func<Supplier, bool>> filter = null, string includes = null)
        {
            return await _unit.Supplier.GetAllAsync(filter, includes);
        }
        public async Task<Supplier> GetByIdAsync(int id)
        {
            try
            {
                return await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplier by ID: {Id}", id);
                throw;
            }
        }

        public async Task<Supplier> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                return await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == name.Trim().ToLower() && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplier by name: {Name}", name);
                throw;
            }
        }

        public async Task<Supplier> CreateAsync(CreateSupplierVM model)
        {
            try
            {
                // Check for duplicate name
                var existingSupplier = await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted
                );

                if (existingSupplier != null)
                {
                    throw new InvalidOperationException("يوجد عميل بنفس الاسم");
                }

                // Map ViewModel to Entity
                var Supplier = _mapper.Map<Supplier>(model);
                Supplier.Balance ??= 0;
                // Add to database
                _unit.Supplier.Add(Supplier);
                await _unit.CompleteAsync();

                _logger.LogInformation("Supplier created successfully: {Name}", Supplier.Name);

                return Supplier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Supplier: {Name}", model?.Name);
                throw;
            }
        }

        public async Task<Supplier> UpdateAsync(EditSupplierVM model)
        {
            try
            {
                // Get existing Supplier
                var Supplier = await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Id == model.Id && !x.IsDeleted
                );

                if (Supplier == null)
                {
                    throw new InvalidOperationException("العميل غير موجود");
                }

                // Check for duplicate name (excluding current Supplier)
                var duplicateSupplier = await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower()
                         && x.Id != model.Id
                         && !x.IsDeleted
                );

                if (duplicateSupplier != null)
                {
                    throw new InvalidOperationException("هذا الاسم مستخدم من عميل آخر");
                }

                // Update properties
                Supplier.Name = model.Name.Trim();
                Supplier.Phone = model.Phone?.Trim();
                Supplier.Address = model.Address?.Trim();
                Supplier.UpdatedAt = DateTime.UtcNow;
                // Save changes
                _unit.Supplier.Update(Supplier);
                await _unit.CompleteAsync();

                _logger.LogInformation("Supplier updated successfully: {Id}", Supplier.Id);

                return Supplier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supplier: {Id}", model?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var Supplier = await _unit.Supplier.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );

                if (Supplier == null)
                {
                    return false;
                }

                if ((Supplier.Balance ?? 0m) != 0m)
                {
                    throw new InvalidOperationException("لا يمكن حذف المورد طالما أن الرصيد لا يساوي صفر");
                }

                // Soft delete
                Supplier.IsDeleted = true;
                Supplier.UpdatedAt = DateTime.UtcNow;

                _unit.Supplier.Update(Supplier);
                await _unit.CompleteAsync();

                _logger.LogInformation("Supplier deleted successfully: {Id}", Supplier.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Supplier: {Id}", id);
                throw;
            }
        }
    }
}
