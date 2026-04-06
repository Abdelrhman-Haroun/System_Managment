using BLL.ViewModels.Employee;
using DAL.Models;

namespace BLL.Services.IService
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllAsync(string? searchTerm = null);
        Task<IEnumerable<Employee>> GetActiveAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee> CreateAsync(CreateEmployeeVM model);
        Task<Employee> UpdateAsync(EditEmployeeVM model);
        Task<bool> DeleteAsync(int id);
    }
}
