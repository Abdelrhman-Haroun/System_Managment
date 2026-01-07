using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;

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
        public IUserRepository User { get; private set; }

        public UnitOfWork(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            Product = new ProductRepository(context);
            Store = new StoreRepository(context);
            ProductCategory = new ProductCategoryRepository(context);
            Supplier = new SupplierRepository(context);
            Customer = new CustomerRepository(context);
            User = new UserRepository(userManager);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
