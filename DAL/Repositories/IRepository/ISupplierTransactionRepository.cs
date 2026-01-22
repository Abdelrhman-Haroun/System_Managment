using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public interface ISupplierTransactionRepository : IGenericRepository<SupplierTransaction>
    {
        Task<IEnumerable<SupplierTransaction>> GetBySupplierIdAsync(int supplierId);
        Task<IEnumerable<SupplierTransaction>> GetByInvoiceIdAsync(int invoiceId);
    }
}
