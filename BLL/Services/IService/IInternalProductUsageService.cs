using BLL.ViewModels.InternalUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IService
{
    public interface IInternalProductUsageService
    {
        Task<(bool Success, string Message, int? UsageId)> RecordInternalUsageAsync(CreateInternalUsageVM model);
        Task<(bool Success, string Message)> UpdateInternalUsageAsync(UpdateInternalUsageVM model);
        Task<(bool Success, string Message)> DeleteInternalUsageAsync(int id);
        Task<InternalUsageDetailsVM> GetUsageDetailsAsync(int id);
        Task<IEnumerable<InternalUsageDetailsVM>> GetAllInternalUsageAsync();
        Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByProductAsync(int productId);
        Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByCategoryAsync(string category);
        Task<(decimal TotalCost, decimal TotalQuantity)> GetProductUsageSummaryAsync(int productId);
        Task<(decimal TotalCost, int RecordCount)> GetMonthlyUsageSummaryAsync(int month, int year);
    }
}
