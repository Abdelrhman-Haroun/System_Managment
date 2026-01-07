
using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Store;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class StoreService : IStoreService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly ILogger<StoreService> _logger;

        public StoreService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<StoreService> logger)
        {
            _unit = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Store>> GetAllAsync(string searchTerm = null)
        {
            try
            {
                var Stores = await _unit.Store.GetAllAsync(c => !c.IsDeleted);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    Stores = Stores.Where(c =>
                        c.Name.ToLower().Contains(term)
                    );
                }

                return Stores.OrderBy(c => c.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Stores");
                throw;
            }
        }

        public async Task<IEnumerable<Store>> GetAllAsync(Expression<Func<Store, bool>> filter = null, string includes = null)
        {
            return await _unit.Store.GetAllAsync(filter, includes);
        }

        public async Task<Store> GetByIdAsync(int id)
        {
            try
            {
                return await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Store by ID: {Id}", id);
                throw;
            }
        }

        public async Task<Store> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                return await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == name.Trim().ToLower() && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Store by name: {Name}", name);
                throw;
            }
        }

        public async Task<Store> CreateAsync(CreateStoreVM model)
        {
            try
            {
                // Check for duplicate name
                var existingStore = await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted
                );

                if (existingStore != null)
                {
                    throw new InvalidOperationException("يوجد مخزن بنفس الاسم");
                }

                // Map ViewModel to Entity
                var Store = _mapper.Map<Store>(model);
                // Add to database
                _unit.Store.Add(Store);
                await _unit.CompleteAsync();

                _logger.LogInformation("Store created successfully: {Name}", Store.Name);

                return Store;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Store: {Name}", model?.Name);
                throw;
            }
        }

        public async Task<Store> UpdateAsync(EditStoreVM model)
        {
            try
            {
                // Get existing Store
                var Store = await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Id == model.Id && !x.IsDeleted
                );

                if (Store == null)
                {
                    throw new InvalidOperationException("المخزن غير موجود");
                }

                // Check for duplicate name (excluding current Store)
                var duplicateStore = await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower()
                         && x.Id != model.Id
                         && !x.IsDeleted
                );

                if (duplicateStore != null)
                {
                    throw new InvalidOperationException("هذا الاسم مستخدم من مخزن آخر");
                }

                // Update properties
                Store.Name = model.Name.Trim();
                Store.Description = model.Description?.Trim();

                // Save changes
                _unit.Store.Update(Store);
                await _unit.CompleteAsync();

                _logger.LogInformation("Store updated successfully: {Id}", Store.Id);

                return Store;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Store: {Id}", model?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var Store = await _unit.Store.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );

                if (Store == null)
                {
                    return false;
                }

                // Soft delete
                Store.IsDeleted = true;

                _unit.Store.Update(Store);
                await _unit.CompleteAsync();

                _logger.LogInformation("Store deleted successfully: {Id}", Store.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Store: {Id}", id);
                throw;
            }
        }


    }
}
