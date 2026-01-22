using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public interface IInternalProductUsageRepository : IGenericRepository<InternalProductUsage>
    {
        Task<IEnumerable<InternalProductUsage>> GetAllWithProductAsync();
        Task<IEnumerable<InternalProductUsage>> GetByProductIdAsync(int productId);
        Task<IEnumerable<InternalProductUsage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InternalProductUsage>> GetByUsageCategoryAsync(string category);
        Task<decimal> GetTotalCostByProductAsync(int productId);
        Task<decimal> GetTotalCostByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> GenerateReferenceNumberAsync();
    }
}
