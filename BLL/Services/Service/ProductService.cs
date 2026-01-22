using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Product;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _unit = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(string searchTerm = null, string includes = null)
        {
            try
            {
                var products = await _unit.Product.GetAllAsync(p => !p.IsDeleted, includes);

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    products = products.Where(p =>
                        p.Name.ToLower().Contains(term) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(term))
                    );
                }

                return products.OrderBy(p => p.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var products = await _unit.Product.GetAllAsync(p => !p.IsDeleted, null);
                return products.OrderBy(p => p.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            try
            {
                return await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by ID: {Id}", id);
                throw;
            }
        }
        public async Task<Product?> GetByIdContainsAsync(
                int id,
                string includes)
        {
            try
            {
                var product = await _unit.Product
                    .GetByIdContainsAsync(id, includes);

                if (product == null || product.IsDeleted)
                    return null;

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                throw;
            }
        }


        public async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null, string includes = null)
        {
            return await _unit.Product.GetAllAsync(filter, includes);
        }

        public async Task<Product> GetByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                return await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == name.Trim().ToLower() && !x.IsDeleted
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Product by name: {Name}", name);
                throw;
            }
        }

        public async Task<Product> CreateAsync(CreateProductVM model)
        {
            try
            {
                // Check for duplicate name
                var existingProduct = await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower() && !x.IsDeleted
                );

                if (existingProduct != null)
                {
                    throw new InvalidOperationException("يوجد منتج بنفس الاسم");
                }

                // Map ViewModel to Entity
                var Product = _mapper.Map<Product>(model);

                // Add to database
                _unit.Product.Add(Product);
                await _unit.CompleteAsync();

                _logger.LogInformation("Product created successfully: {Name}", Product.Name);

                return Product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Product: {Name}", model?.Name);
                throw;
            }
        }

        public async Task<Product> UpdateAsync(EditProductVM model)
        {
            try
            {
                // Get existing Product
                var Product = await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Id == model.Id && !x.IsDeleted
                );

                if (Product == null)
                {
                    throw new InvalidOperationException("المنتج غير موجود");
                }

                // Check for duplicate name (excluding current Product)
                var duplicateProduct = await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Name.ToLower() == model.Name.Trim().ToLower()
                         && x.Id != model.Id
                         && !x.IsDeleted
                );

                if (duplicateProduct != null)
                {
                    throw new InvalidOperationException("هذا الاسم مستخدم من منتج آخر");
                }

                // Update properties
                Product.Name = model.Name.Trim();
                Product.Description = model.Description?.Trim();
                Product.StoreId = model.StoreId;
                Product.CategoryId = model.CategoryId;
                Product.UpdatedAt = DateTime.UtcNow;


                // Save changes
                _unit.Product.Update(Product);
                await _unit.CompleteAsync();

                _logger.LogInformation("Product updated successfully: {Id}", Product.Id);

                return Product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Product: {Id}", model?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var Product = await _unit.Product.GetFirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted
                );

                if (Product == null)
                {
                    return false;
                }

                // Soft delete
                Product.IsDeleted = true;

                _unit.Product.Update(Product);
                await _unit.CompleteAsync();

                _logger.LogInformation("Product deleted successfully: {Id}", Product.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Product: {Id}", id);
                throw;
            }
        }

    }
}