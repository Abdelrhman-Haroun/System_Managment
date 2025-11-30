
using BLL.Services.IService;
using DAL.Repositories.IRepository;
using DAL.Models;
using System.Linq.Expressions;

namespace BLL.Services.Service
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unit;

        public ProductCategoryService(IUnitOfWork unitOfWork)
        {
            _unit = unitOfWork;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(Expression<Func<ProductCategory, bool>> filter = null)
        {
            return await _unit.ProductCategory.GetAllAsync(filter);
        }

        public async Task<ProductCategory> GetByIdAsync(int id)
        {
            return await _unit.ProductCategory.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        } 
        public async Task<ProductCategory> GetByNameAsync(string name)
        {
            return await _unit.ProductCategory.GetFirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
        }

        public async Task<ProductCategory> CreateAsync(ProductCategory ProductCategory)
        {
            _unit.ProductCategory.Add(ProductCategory);
            _unit.Complete();
            return ProductCategory;
        }

        public async Task<ProductCategory> UpdateAsync(ProductCategory ProductCategory)
        {
            _unit.ProductCategory.Update(ProductCategory); 
            _unit.Complete();
            return ProductCategory;
        }

        public async Task DeleteAsync(int id)
        {
            var ProductCategory =await _unit.ProductCategory.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (ProductCategory != null)
            {
                //_unit.ProductCategory.Remove(ProductCategory);
                ProductCategory.IsDeleted = true;
                _unit.Complete();
            }
        }
    }
}
