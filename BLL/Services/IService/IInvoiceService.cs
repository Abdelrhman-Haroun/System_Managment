using BLL.ViewModels.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IService
{
    public interface IInvoiceService
    {
        Task<(bool Success, string Message, int? InvoiceId)> CreateInvoiceAsync(CreateInvoiceVM model);
        Task<(bool Success, string Message)> UpdateInvoiceAsync(UpdateInvoiceVM model);
        Task<(bool Success, string Message)> DeleteInvoiceAsync(int id);
        Task<InvoiceDetailsVM?> GetInvoiceDetailsAsync(int id);
        Task<IEnumerable<InvoiceListVM>> GetAllInvoicesAsync(string? invoiceType = null);
    }
}
