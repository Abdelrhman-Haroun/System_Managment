using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace DAL.Repositories.Repository
{
    public class EmployeeAttendanceRepository : GenericRepository<EmployeeAttendance>, IEmployeeAttendanceRepository
    {
        public EmployeeAttendanceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
