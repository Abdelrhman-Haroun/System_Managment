using BLL.ViewModels.Payment;
using DAL.Models;

namespace BLL.Services.IService
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentListVM>> GetAllAsync(string? searchTerm = null);
        Task<PaymentFormVM?> GetForEditAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(PaymentFormVM model);
        Task<(bool Success, string Message)> UpdateAsync(PaymentFormVM model);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<IEnumerable<Supplier>> GetSuppliersAsync();
    }
}
