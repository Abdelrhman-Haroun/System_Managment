
using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.ProductCategory;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductCategoryService> _logger;

        public ProductCategoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductCategoryService> logger)
        {
            _unit = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(string searchTerm = null)
        {
            try
            {
                var ProductCategorys = await _unit.ProductCategory.GetAllAsync(c => !c.IsDeleted);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    ProductCategorys = ProductCategorys.Where(c =>
                        c.Name.ToLower().Contains(term)
                    );
                }

                return ProductCategorys.OrderBy(c => c.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProductCategorys");
                throw;
            }
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(Expression<Func<ProductCategory, bool>> filter = null, string includes = null)
        {
            return await _unit.ProductCategory.GetAllAsync(filter, includes);
        }

        public async Task<ProductCategory> GetByIdAsync(int id)
        {
            try
            {
                return await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProductCategory by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ProductCategory> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                return await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == name.Trim().ToLower() && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProductCategory by name: {Name}", name);
                throw;
            }
        }

        public async Task<ProductCategory> CreateAsync(CreateProductCategoryVM model)
        {
            try
            {
                // Check for duplicate name
                var existingProductCategory = await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted
                );

                if (existingProductCategory != null)
                {
                    throw new InvalidOperationException("يوجد فئة منتج بنفس الاسم");
                }

                // Map ViewModel to Entity
                var ProductCategory = _mapper.Map<ProductCategory>(model);
                // Add to database
                _unit.ProductCategory.Add(ProductCategory);
                await _unit.CompleteAsync();

                _logger.LogInformation("ProductCategory created successfully: {Name}", ProductCategory.Name);

                return ProductCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ProductCategory: {Name}", model?.Name);
                throw;
            }
        }

        public async Task<ProductCategory> UpdateAsync(EditProductCategoryVM model)
        {
            try
            {
                // Get existing ProductCategory
                var ProductCategory = await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Id == model.Id && !x.IsDeleted
                );

                if (ProductCategory == null)
                {
                    throw new InvalidOperationException("فئة المنتج غير موجود");
                }

                // Check for duplicate name (excluding current ProductCategory)
                var duplicateProductCategory = await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower()
                         && x.Id != model.Id
                         && !x.IsDeleted
                );

                if (duplicateProductCategory != null)
                {
                    throw new InvalidOperationException("هذا الاسم مستخدم من فئة منتج آخرى");
                }

                // Update properties
                ProductCategory.Name = model.Name.Trim();
                ProductCategory.Description = model.Description?.Trim();

                // Save changes
                _unit.ProductCategory.Update(ProductCategory);
                await _unit.CompleteAsync();

                _logger.LogInformation("ProductCategory updated successfully: {Id}", ProductCategory.Id);

                return ProductCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ProductCategory: {Id}", model?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var ProductCategory = await _unit.ProductCategory.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );

                if (ProductCategory == null)
                {
                    return false;
                }

                // Soft delete
                ProductCategory.IsDeleted = true;

                _unit.ProductCategory.Update(ProductCategory);
                await _unit.CompleteAsync();

                _logger.LogInformation("ProductCategory deleted successfully: {Id}", ProductCategory.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ProductCategory: {Id}", id);
                throw;
            }
        }

     
    }
}
