using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Repository
{
    public class SupplierTransactionRepository : GenericRepository<SupplierTransaction>, ISupplierTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public SupplierTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierTransaction>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.SupplierTransactions.Where(st => st.SupplierId == supplierId && !st.IsDeleted)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupplierTransaction>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.SupplierTransactions.Where(st => st.InvoiceId == invoiceId && !st.IsDeleted)
                .OrderByDescending(st => st.CreatedAt)
                .ToListAsync();
        }
    }
}

