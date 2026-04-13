using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Repository
{
    public class EmployeeTransactionRepository : GenericRepository<EmployeeTransaction>, IEmployeeTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeTransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
