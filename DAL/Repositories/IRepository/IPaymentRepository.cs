using DAL.Models;

namespace DAL.Repositories.IRepository
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetAllWithDetailsAsync();
        Task<Payment?> GetByIdWithDetailsAsync(int id);
    }
}
