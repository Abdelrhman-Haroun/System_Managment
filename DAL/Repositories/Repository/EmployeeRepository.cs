using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace DAL.Repositories.Repository
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
