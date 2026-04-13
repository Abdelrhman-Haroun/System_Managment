using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using DAL.Repositories.Repository;
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
        public IPaymentRepository Payment { get; private set; }
        public IUserRepository User { get; private set; }
        public IInvoiceRepository Invoice { get; private set; }
        public IProductTransactionRepository ProductTransaction { get; private set; }
        public ICustomerTransactionRepository CustomerTransaction { get; private set; }
        public ISupplierTransactionRepository SupplierTransaction { get; private set; }
        public IInternalProductUsageRepository InternalProductUsage { get; private set; }
        public IEmployeeRepository Employee { get; private set; }
        public IEmployeeTypeRepository EmployeeType { get; private set; }
        public IEmployeeAttendanceRepository EmployeeAttendance { get; private set; }
        public IEmployeeSalaryAdjustmentRepository EmployeeSalaryAdjustment { get; private set; }
        public IEmployeeSalaryHistoryRepository EmployeeSalaryHistory { get; private set; }
        public IPaymentMethodTransactionRepository PaymentMethodTransaction { get; private set; }
        public IEmployeeTransactionRepository EmployeeTransaction { get; private set; }

        public UnitOfWork(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            User = new UserRepository(userManager);
            Product = new ProductRepository(context);
            Store = new StoreRepository(context);
            ProductCategory = new ProductCategoryRepository(context);
            Supplier = new SupplierRepository(context);
            Customer = new CustomerRepository(context);
            Payment = new PaymentRepository(context);
            Invoice = new InvoiceRepository(context);
            ProductTransaction = new ProductTransactionRepository(context);
            CustomerTransaction = new CustomerTransactionRepository(context);
            SupplierTransaction = new SupplierTransactionRepository(context);
            InternalProductUsage = new InternalProductUsageRepository(context);
            Employee = new EmployeeRepository(context);
            EmployeeType = new EmployeeTypeRepository(context);
            EmployeeAttendance = new EmployeeAttendanceRepository(context);
            EmployeeSalaryAdjustment = new EmployeeSalaryAdjustmentRepository(context);
            EmployeeSalaryHistory = new EmployeeSalaryHistoryRepository(context);
            PaymentMethodTransaction = new PaymentMethodTransactionRepository(context);
            EmployeeTransaction = new EmployeeTransactionRepository(context);
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
