using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public class PaymentMethodTransactionRepository : GenericRepository<PaymentMethodTransaction>, IPaymentMethodTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
