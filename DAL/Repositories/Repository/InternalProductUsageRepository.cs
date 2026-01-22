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
    public class InternalProductUsageRepository : GenericRepository<InternalProductUsage>, IInternalProductUsageRepository
    {
        private readonly ApplicationDbContext _context;
        public InternalProductUsageRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<InternalProductUsage>> GetAllWithProductAsync()
        {
            return await _context.InternalProductUsages
                .Where(u =>!u.IsDeleted)
                .Include(u => u.Product)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<InternalProductUsage>> GetByProductIdAsync(int productId)
        {
            return await _context.InternalProductUsages
                .Where(u => u.ProductId == productId && !u.IsDeleted)
                .Include(u => u.Product)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InternalProductUsage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InternalProductUsages
                .Where(u => u.UsageDate >= startDate && u.UsageDate <= endDate && !u.IsDeleted)
                .Include(u => u.Product)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InternalProductUsage>> GetByUsageCategoryAsync(string category)
        {
            return await _context.InternalProductUsages
                .Where(u => u.UsageCategory == category && !u.IsDeleted)
                .Include(u => u.Product)
                .OrderByDescending(u => u.UsageDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalCostByProductAsync(int productId)
        {
            return await _context.InternalProductUsages
                .Where(u => u.ProductId == productId && !u.IsDeleted)
                .SumAsync(u => u.TotalCost);
        }

        public async Task<decimal> GetTotalCostByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InternalProductUsages
                .Where(u => u.UsageDate >= startDate && u.UsageDate <= endDate && !u.IsDeleted)
                .SumAsync(u => u.TotalCost);
        }

        public async Task<string> GenerateReferenceNumberAsync()
        {
            var prefix =  "Internal-Usage";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            // Get the last invoice number for this type and month
            var lastInvoice = await _context.InternalProductUsages
                .Where(i => 
                           i.UsageDate.Year == year &&
                           i.UsageDate.Month == month)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.ReferenceNumber))
            {
                // Extract number from last invoice (e.g., "PUR-2024-01-0005" -> 5)
                var parts = lastInvoice.ReferenceNumber.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"{prefix}-{year}-{month:D2}-{nextNumber:D4}";
        }
    }
}

