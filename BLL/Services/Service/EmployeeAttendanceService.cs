using BLL.Services.IService;
using BLL.ViewModels.Employee;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace BLL.Services.Service
{
    public class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        private const int PaidOffDaysPerMonth = 4;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeAttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> MarkAttendanceAsync(MarkEmployeeAttendanceVM model)
        {
            if (model.EmployeeId <= 0)
            {
                return (false, "الموظف غير موجود");
            }

            var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(x => x.Id == model.EmployeeId && !x.IsDeleted);
            if (employee == null)
            {
                return (false, "الموظف غير موجود");
            }

            if (model.Status != EmployeeAttendanceStatus.Present && model.Status != EmployeeAttendanceStatus.Absent)
            {
                return (false, "حالة الحضور غير صحيحة");
            }

            var date = model.AttendanceDate.Date;
            var existing = await _unitOfWork.EmployeeAttendance.GetFirstOrDefaultAsync(
                x => x.EmployeeId == model.EmployeeId && x.AttendanceDate == date && !x.IsDeleted);

            if (existing == null)
            {
                _unitOfWork.EmployeeAttendance.Add(new EmployeeAttendance
                {
                    EmployeeId = model.EmployeeId,
                    AttendanceDate = date,
                    Status = model.Status,
                    Notes = model.Notes?.Trim()
                });
            }
            else
            {
                existing.Status = model.Status;
                existing.Notes = model.Notes?.Trim();
                existing.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.EmployeeAttendance.Update(existing);
            }

            await _unitOfWork.CompleteAsync();
            return (true, "تم حفظ الحضور والغياب بنجاح");
        }

        public async Task<(bool Success, string Message)> DeleteAttendanceAsync(int id)
        {
            var attendance = await _unitOfWork.EmployeeAttendance.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (attendance == null)
            {
                return (false, "سجل الحضور غير موجود");
            }

            attendance.IsDeleted = true;
            attendance.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.EmployeeAttendance.Update(attendance);
            await _unitOfWork.CompleteAsync();
            return (true, "تم حذف سجل الحضور");
        }

        public async Task<(bool Success, string Message)> AddSalaryAdjustmentAsync(CreateEmployeeSalaryAdjustmentVM model)
        {
            if (model.EmployeeId <= 0)
            {
                return (false, "الموظف غير موجود");
            }

            var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(x => x.Id == model.EmployeeId && !x.IsDeleted);
            if (employee == null)
            {
                return (false, "الموظف غير موجود");
            }

            if (model.AdjustmentType != SalaryAdjustmentTypes.Addition && model.AdjustmentType != SalaryAdjustmentTypes.Deduction)
            {
                return (false, "نوع الإضافة أو الخصم غير صحيح");
            }

            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return (false, "السبب مطلوب");
            }

            _unitOfWork.EmployeeSalaryAdjustment.Add(new EmployeeSalaryAdjustment
            {
                EmployeeId = model.EmployeeId,
                AdjustmentDate = model.AdjustmentDate.Date,
                AdjustmentType = model.AdjustmentType,
                Amount = model.Amount,
                Reason = model.Reason.Trim(),
                Notes = model.Notes?.Trim()
            });

            await _unitOfWork.CompleteAsync();
            return (true, "تم حفظ الإضافة أو الخصم بنجاح");
        }

        public async Task<(bool Success, string Message)> DeleteSalaryAdjustmentAsync(int id)
        {
            var adjustment = await _unitOfWork.EmployeeSalaryAdjustment.GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (adjustment == null)
            {
                return (false, "الحركة غير موجودة");
            }

            adjustment.IsDeleted = true;
            adjustment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.EmployeeSalaryAdjustment.Update(adjustment);
            await _unitOfWork.CompleteAsync();
            return (true, "تم حذف الحركة");
        }

        public async Task<IEnumerable<EmployeeAttendanceRecordVM>> GetAttendanceRecordsAsync(int year, int month, int? employeeId = null)
        {
            var (startDate, endDate) = GetMonthBounds(year, month);
            var records = await _unitOfWork.EmployeeAttendance.GetAllAsync(
                x => !x.IsDeleted &&
                     x.AttendanceDate >= startDate &&
                     x.AttendanceDate <= endDate &&
                     (!employeeId.HasValue || x.EmployeeId == employeeId.Value),
                "Employee");

            return records
                .OrderByDescending(x => x.AttendanceDate)
                .ThenBy(x => x.Employee!.Name)
                .Select(x => new EmployeeAttendanceRecordVM
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee?.Name ?? string.Empty,
                    AttendanceDate = x.AttendanceDate,
                    Status = x.Status,
                    Notes = x.Notes
                })
                .ToList();
        }

        public async Task<IEnumerable<EmployeeSalaryAdjustmentVM>> GetSalaryAdjustmentsAsync(int year, int month, int? employeeId = null)
        {
            var (startDate, endDate) = GetMonthBounds(year, month);
            var adjustments = await _unitOfWork.EmployeeSalaryAdjustment.GetAllAsync(
                x => !x.IsDeleted &&
                     x.AdjustmentDate >= startDate &&
                     x.AdjustmentDate <= endDate &&
                     (!employeeId.HasValue || x.EmployeeId == employeeId.Value),
                "Employee");

            return adjustments
                .OrderByDescending(x => x.AdjustmentDate)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new EmployeeSalaryAdjustmentVM
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee?.Name ?? string.Empty,
                    AdjustmentDate = x.AdjustmentDate,
                    AdjustmentType = x.AdjustmentType,
                    Amount = x.Amount,
                    Reason = x.Reason,
                    Notes = x.Notes
                })
                .ToList();
        }

        public async Task<IEnumerable<EmployeeMonthlyPayrollVM>> GetMonthlyPayrollAsync(int year, int month, int? employeeId = null)
        {
            var (startDate, endDate) = GetMonthBounds(year, month);

            var employees = await _unitOfWork.Employee.GetAllAsync(
                x => !x.IsDeleted && x.IsActive && (!employeeId.HasValue || x.Id == employeeId.Value),
                "EmployeeType");

            var attendanceRecords = (await _unitOfWork.EmployeeAttendance.GetAllAsync(
                x => !x.IsDeleted && x.AttendanceDate >= startDate && x.AttendanceDate <= endDate)).ToList();

            var adjustments = (await _unitOfWork.EmployeeSalaryAdjustment.GetAllAsync(
                x => !x.IsDeleted && x.AdjustmentDate >= startDate && x.AdjustmentDate <= endDate)).ToList();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var payroll = new List<EmployeeMonthlyPayrollVM>();

            foreach (var employee in employees.OrderBy(x => x.Name))
            {
                var employeeAttendance = attendanceRecords.Where(x => x.EmployeeId == employee.Id).ToList();
                var employeeAdjustments = adjustments.Where(x => x.EmployeeId == employee.Id).ToList();

                var presentDays = employeeAttendance.Count(x => x.Status == EmployeeAttendanceStatus.Present);
                var absentDays = employeeAttendance.Count(x => x.Status == EmployeeAttendanceStatus.Absent);

                var dailyRate = daysInMonth == 0 ? 0 : Math.Round(employee.Salary / daysInMonth, 2);

                int paidOffDays;

                if (absentDays == 0 && presentDays == daysInMonth)
                {
                    paidOffDays = PaidOffDaysPerMonth;
                }
                else
                {
                    paidOffDays = Math.Min(absentDays, PaidOffDaysPerMonth);
                }

                var earnedDays = presentDays + paidOffDays;
                var earnedSalary = earnedDays * dailyRate;

                var unpaidAbsentDays = Math.Max(0, absentDays - PaidOffDaysPerMonth);

                var additions = employeeAdjustments
                    .Where(x => x.AdjustmentType == SalaryAdjustmentTypes.Addition)
                    .Sum(x => x.Amount);

                var deductions = employeeAdjustments
                    .Where(x => x.AdjustmentType == SalaryAdjustmentTypes.Deduction)
                    .Sum(x => x.Amount);

                var netSalary = earnedSalary
                                + additions
                                - deductions;

                payroll.Add(new EmployeeMonthlyPayrollVM
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    EmployeeTypeName = employee.EmployeeType?.Name ?? "غير محدد",
                    Position = employee.Position,
                    Salary = employee.Salary,
                    DailyRate = dailyRate,
                    EarnedSalary = earnedSalary,
                    DaysInMonth = daysInMonth,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    PaidOffDaysAllowance = PaidOffDaysPerMonth,
                    ExtraWorkedOffDays = (absentDays == 0 && presentDays == daysInMonth) ? PaidOffDaysPerMonth : 0,
                    UnpaidAbsentDays = unpaidAbsentDays,
                    OffDaysCompensation = paidOffDays * dailyRate,
                    AbsenceDeduction = 0m,
                    Additions = additions,
                    Deductions = deductions,
                    NetSalary = netSalary
                });
            }

            return payroll;
        }


        private static (DateTime StartDate, DateTime EndDate) GetMonthBounds(int year, int month)
        {
            var safeYear = year < 2000 ? DateTime.Today.Year : year;
            var safeMonth = month is < 1 or > 12 ? DateTime.Today.Month : month;
            var startDate = new DateTime(safeYear, safeMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return (startDate, endDate);
        }
    }
}
