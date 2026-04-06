using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace DAL.Repositories.Repository
{
    public class EmployeeTypeRepository : GenericRepository<EmployeeType>, IEmployeeTypeRepository
    {
        public EmployeeTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
