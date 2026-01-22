using DAL.Models;
using System.Linq.Expressions;

namespace DAL.Repositories.IRepository
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<string> GenerateInvoiceNumberAsync(string invoiceType);
        Task<Invoice?> GetInvoiceWithDetailsAsync(int id);
        Task<IEnumerable<Invoice>> GetAllWithDetailsAsync(string? invoiceType = null);
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetBySupplierIdAsync(int supplierId);
    }
}
