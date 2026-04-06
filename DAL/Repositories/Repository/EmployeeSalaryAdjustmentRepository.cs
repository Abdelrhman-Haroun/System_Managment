using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace DAL.Repositories.Repository
{
    public class EmployeeSalaryAdjustmentRepository : GenericRepository<EmployeeSalaryAdjustment>, IEmployeeSalaryAdjustmentRepository
    {
        public EmployeeSalaryAdjustmentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
