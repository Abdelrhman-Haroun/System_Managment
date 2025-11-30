using BLL.Services.IService;
using DAL.Repositories.IRepository;
using System.Linq.Expressions;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unit;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unit = unitOfWork;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, bool>> filter = null, string includes = null)
    {
        return await _unit.Product.GetAllAsync(filter, includes);
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _unit.Product.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, "Store,Category");
    }

    public async Task<Product> GetByNameAsync(string name)
    {
        return await _unit.Product.GetFirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted, "Store,Category");
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _unit.Product.Add(product);
        _unit.Complete();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _unit.Product.Update(product);
        _unit.Complete();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _unit.Product.GetFirstOrDefaultAsync(x => x.Id == id&& !x.IsDeleted);
        if (product != null)
        {
            product.IsDeleted = true;
            _unit.Complete();
        }
    }
}
