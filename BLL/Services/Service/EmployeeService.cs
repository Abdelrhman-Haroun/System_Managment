using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Employee;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EmployeeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(string? searchTerm = null)
        {
            var employees = await _unitOfWork.Employee.GetAllAsync(x => !x.IsDeleted, "EmployeeType");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                employees = employees.Where(x =>
                    x.Name.ToLower().Contains(term) ||
                    (!string.IsNullOrWhiteSpace(x.PhoneNumber) && x.PhoneNumber.Contains(term)) ||
                    (!string.IsNullOrWhiteSpace(x.Position) && x.Position.ToLower().Contains(term)) ||
                    (x.EmployeeType != null && x.EmployeeType.Name.ToLower().Contains(term)));
            }

            return employees.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<IEnumerable<Employee>> GetActiveAsync()
        {
            var employees = await _unitOfWork.Employee.GetAllAsync(x => !x.IsDeleted && x.IsActive, "EmployeeType");
            return employees.OrderBy(x => x.Name).ToList();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Employee.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, "EmployeeType");
        }

        public async Task<Employee> CreateAsync(CreateEmployeeVM model)
        {
            await EnsureEmployeeTypeExists(model.EmployeeTypeId);

            var employee = _mapper.Map<Employee>(model);
            employee.Name = model.Name.Trim();
            employee.PhoneNumber = model.PhoneNumber?.Trim();
            employee.Position = model.Position?.Trim();
            employee.Notes = model.Notes?.Trim();
            employee.HireDate = model.HireDate.Date;

            _unitOfWork.Employee.Add(employee);
            await _unitOfWork.CompleteAsync();

            // Create initial salary history snapshot effective from hire date or creation date
            var effective = employee.HireDate != default ? employee.HireDate : employee.CreatedAt;
            _unitOfWork.EmployeeSalaryHistory.Add(new DAL.Models.EmployeeSalaryHistory
            {
                EmployeeId = employee.Id,
                Salary = employee.Salary,
                EffectiveFrom = effective,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _unitOfWork.CompleteAsync();

            return employee;
        }

        public async Task<Employee> UpdateAsync(EditEmployeeVM model)
        {
            var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
            if (employee == null)
            {
                throw new InvalidOperationException("الموظف غير موجود");
            }

            await EnsureEmployeeTypeExists(model.EmployeeTypeId);

            employee.Name = model.Name.Trim();
            employee.PhoneNumber = model.PhoneNumber?.Trim();

            var oldSalary = employee.Salary;
            if (oldSalary != model.Salary)
            {
                // Salary changes apply to future payroll periods only.
                employee.Salary = model.Salary;
                var effectiveFrom = GetNextPayrollEffectiveDate();
                _unitOfWork.EmployeeSalaryHistory.Add(new DAL.Models.EmployeeSalaryHistory
                {
                    EmployeeId = employee.Id,
                    Salary = model.Salary,
                    EffectiveFrom = effectiveFrom,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            
            employee.EmployeeTypeId = model.EmployeeTypeId;
            employee.Position = model.Position?.Trim();
            employee.HireDate = model.HireDate.Date;
            employee.Notes = model.Notes?.Trim();
            employee.IsActive = model.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Employee.Update(employee);
            await _unitOfWork.CompleteAsync();
            return employee;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (employee == null)
            {
                return false;
            }

            if ((employee.Balance ?? 0m) != 0m)
            {
                throw new InvalidOperationException("لا يمكن حذف الموظف طالما أن الرصيد لا يساوي صفر");
            }

            employee.IsDeleted = true;
            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Employee.Update(employee);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        private static DateTime GetNextPayrollEffectiveDate()
        {
            var today = DateTime.UtcNow.Date;
            return new DateTime(today.Year, today.Month, 1).AddMonths(1);
        }

        private async Task EnsureEmployeeTypeExists(int? employeeTypeId)
        {
            if (!employeeTypeId.HasValue)
            {
                return;
            }

            var exists = await _unitOfWork.EmployeeType.AnyAsync(x => x.Id == employeeTypeId.Value && !x.IsDeleted);
            if (!exists)
            {
                throw new InvalidOperationException("نوع الموظف المحدد غير موجود");
            }
        }
    }
}
