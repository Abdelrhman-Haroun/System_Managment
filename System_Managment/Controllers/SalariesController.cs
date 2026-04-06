using BLL.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace System_Managment.Controllers;

[Authorize]
public class SalariesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeAttendanceService _attendanceService;

    public SalariesController(IEmployeeService employeeService, IEmployeeAttendanceService attendanceService)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? employeeId, int? year, int? month)
    {
        var selectedYear = year ?? DateTime.Today.Year;
        var selectedMonth = month ?? DateTime.Today.Month;

        var employees = await _employeeService.GetActiveAsync();
        ViewBag.Employees = new SelectList(employees, "Id", "Name", employeeId);
        ViewBag.SelectedEmployeeId = employeeId;
        ViewBag.SelectedYear = selectedYear;
        ViewBag.SelectedMonth = selectedMonth;

        var payroll = await _attendanceService.GetMonthlyPayrollAsync(selectedYear, selectedMonth, employeeId);
        return View(payroll);
    }
}
