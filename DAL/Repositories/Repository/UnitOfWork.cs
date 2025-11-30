using DAL.Data;
using DAL.Repositories;

namespace DAL.Repositories.IRepository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IProductRepository Product { get; private set; }       
        public IStoreRepository Store { get; private set; }       
        public IProductCategoryRepository ProductCategory { get; private set; }       
        public ISupplierRepository Supplier { get; private set; }
        public ICustomerRepository Customer { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Product = new ProductRepository(context);
            Store = new StoreRepository(context);
            ProductCategory = new ProductCategoryRepository(context);
            Supplier = new SupplierRepository(context);
            Customer = new CustomerRepository(context);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
