using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public interface ICustomerTransactionRepository : IGenericRepository<CustomerTransaction>
    {
        Task<IEnumerable<CustomerTransaction>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerTransaction>> GetByInvoiceIdAsync(int invoiceId);
    }
}
