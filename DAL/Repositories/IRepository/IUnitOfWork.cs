
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository User { get; }
        IProductRepository Product { get; }
        IStoreRepository Store { get; }
        IProductCategoryRepository ProductCategory { get; }
        ISupplierRepository Supplier { get; }
        ICustomerRepository Customer { get; }
        IPaymentRepository Payment { get; }
        IInvoiceRepository Invoice { get; }
        IProductTransactionRepository ProductTransaction { get; }
        ICustomerTransactionRepository CustomerTransaction { get; }
        ISupplierTransactionRepository SupplierTransaction { get; }
        IInternalProductUsageRepository InternalProductUsage { get; }
        IEmployeeRepository Employee { get; }
        IEmployeeTypeRepository EmployeeType { get; }
        IEmployeeAttendanceRepository EmployeeAttendance { get; }
        IEmployeeSalaryAdjustmentRepository EmployeeSalaryAdjustment { get; }
        IEmployeeSalaryHistoryRepository EmployeeSalaryHistory { get; }
        IPaymentMethodTransactionRepository PaymentMethodTransaction { get; }
        IEmployeeTransactionRepository EmployeeTransaction { get; }
        Task<int> CompleteAsync();
    }
}
