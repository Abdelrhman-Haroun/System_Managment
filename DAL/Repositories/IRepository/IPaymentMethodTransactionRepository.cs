using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.IRepository
{
    public interface IPaymentMethodTransactionRepository : IGenericRepository<PaymentMethodTransaction>
    {
    }
}
