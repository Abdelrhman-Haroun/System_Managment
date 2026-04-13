using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace DAL.Repositories.Repository
{
    public class EmployeeSalaryHistoryRepository : GenericRepository<EmployeeSalaryHistory>, IEmployeeSalaryHistoryRepository
    {
        public EmployeeSalaryHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}