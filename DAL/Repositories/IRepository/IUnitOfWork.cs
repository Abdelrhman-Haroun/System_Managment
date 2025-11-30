
namespace DAL.Repositories.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Product { get; }
        IStoreRepository Store { get; }
        IProductCategoryRepository ProductCategory { get; }
        ISupplierRepository Supplier { get; }
        ICustomerRepository Customer { get; }
        int Complete();
    }
}
