using BLL.ViewModels.EmployeeType;
using DAL.Models;

namespace BLL.Services.IService
{
    public interface IEmployeeTypeService
    {
        Task<IEnumerable<EmployeeType>> GetAllAsync(string? searchTerm = null);
        Task<EmployeeType?> GetByIdAsync(int id);
        Task<EmployeeType> CreateAsync(CreateEmployeeTypeVM model);
        Task<EmployeeType> UpdateAsync(EditEmployeeTypeVM model);
        Task<bool> DeleteAsync(int id);
    }
}
