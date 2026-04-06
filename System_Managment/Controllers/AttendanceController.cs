using BLL.Services.IService;
using BLL.ViewModels.Employee;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace System_Managment.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeAttendanceService _attendanceService;

    public AttendanceController(IEmployeeService employeeService, IEmployeeAttendanceService attendanceService)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? employeeId, int? year, int? month)
    {
        var selectedYear = year ?? DateTime.Today.Year;
        var selectedMonth = month ?? DateTime.Today.Month;

        var employees = (await _employeeService.GetActiveAsync()).ToList();
        var normalizedEmployeeId = employees.Any(x => x.Id == employeeId) ? employeeId : null;
        var defaultEmployeeId = normalizedEmployeeId ?? employees.FirstOrDefault()?.Id ?? 0;

        ViewBag.Employees = new SelectList(employees, "Id", "Name", normalizedEmployeeId);
        ViewBag.SelectedEmployeeId = normalizedEmployeeId;
        ViewBag.SelectedYear = selectedYear;
        ViewBag.SelectedMonth = selectedMonth;
        ViewBag.HasEmployees = employees.Any();
        ViewBag.PresentStatus = EmployeeAttendanceStatus.Present;
        ViewBag.AbsentStatus = EmployeeAttendanceStatus.Absent;
        ViewBag.AdditionType = SalaryAdjustmentTypes.Addition;
        ViewBag.DeductionType = SalaryAdjustmentTypes.Deduction;
        ViewBag.Payroll = await _attendanceService.GetMonthlyPayrollAsync(selectedYear, selectedMonth, normalizedEmployeeId);
        ViewBag.Records = await _attendanceService.GetAttendanceRecordsAsync(selectedYear, selectedMonth, normalizedEmployeeId);
        ViewBag.Adjustments = await _attendanceService.GetSalaryAdjustmentsAsync(selectedYear, selectedMonth, normalizedEmployeeId);

        var model = new MarkEmployeeAttendanceVM
        {
            EmployeeId = defaultEmployeeId,
            AttendanceDate = DateTime.Today,
            Status = EmployeeAttendanceStatus.Present
        };

        ViewBag.AdjustmentModel = new CreateEmployeeSalaryAdjustmentVM
        {
            EmployeeId = defaultEmployeeId,
            AdjustmentDate = DateTime.Today,
            AdjustmentType = SalaryAdjustmentTypes.Addition
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(MarkEmployeeAttendanceVM model)
    {
        var result = await _attendanceService.MarkAttendanceAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId, year = model.AttendanceDate.Year, month = model.AttendanceDate.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecord(int id, int? employeeId, int year, int month)
    {
        var result = await _attendanceService.DeleteAttendanceAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index), new { employeeId, year, month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAdjustment(CreateEmployeeSalaryAdjustmentVM model)
    {
        var result = await _attendanceService.AddSalaryAdjustmentAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId, year = model.AdjustmentDate.Year, month = model.AdjustmentDate.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdjustment(int id, int? employeeId, int year, int month)
    {
        var result = await _attendanceService.DeleteSalaryAdjustmentAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index), new { employeeId, year, month });
    }
}
