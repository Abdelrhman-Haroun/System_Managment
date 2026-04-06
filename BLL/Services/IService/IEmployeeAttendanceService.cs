using BLL.ViewModels.Employee;

namespace BLL.Services.IService
{
    public interface IEmployeeAttendanceService
    {
        Task<(bool Success, string Message)> MarkAttendanceAsync(MarkEmployeeAttendanceVM model);
        Task<(bool Success, string Message)> DeleteAttendanceAsync(int id);
        Task<(bool Success, string Message)> AddSalaryAdjustmentAsync(CreateEmployeeSalaryAdjustmentVM model);
        Task<(bool Success, string Message)> DeleteSalaryAdjustmentAsync(int id);
        Task<IEnumerable<EmployeeAttendanceRecordVM>> GetAttendanceRecordsAsync(int year, int month, int? employeeId = null);
        Task<IEnumerable<EmployeeSalaryAdjustmentVM>> GetSalaryAdjustmentsAsync(int year, int month, int? employeeId = null);
        Task<IEnumerable<EmployeeMonthlyPayrollVM>> GetMonthlyPayrollAsync(int year, int month, int? employeeId = null);
    }
}
