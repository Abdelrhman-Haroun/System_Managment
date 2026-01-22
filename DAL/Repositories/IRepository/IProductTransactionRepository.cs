using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public interface IProductTransactionRepository : IGenericRepository<ProductTransaction>
    {
        Task<IEnumerable<ProductTransaction>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductTransaction>> GetByInvoiceIdAsync(int invoiceId);
    }
}
