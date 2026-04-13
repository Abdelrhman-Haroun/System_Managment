using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Employee;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace System_Managment.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _service;
    private readonly IEmployeeTypeService _employeeTypeService;
    private readonly IEmployeeAttendanceService _attendanceService;
    private readonly IMapper _mapper;
    private readonly ITransactionReportService _transactionReportService;

    public EmployeesController(
        IEmployeeService service,
        IEmployeeTypeService employeeTypeService,
        IEmployeeAttendanceService attendanceService,
        IMapper mapper,
        ITransactionReportService transactionReportService)
    {
        _service = service;
        _employeeTypeService = employeeTypeService;
        _attendanceService = attendanceService;
        _mapper = mapper;
        _transactionReportService = transactionReportService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        try
        {
            var employees = await _service.GetAllAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            return View(employees);
        }
        catch
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل الموظفين";
            return View(new List<DAL.Models.Employee>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEmployeeTypesAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_CreatePartial", new CreateEmployeeVM());

        return View(new CreateEmployeeVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var employee = await _service.CreateAsync(model);
            return Json(new { success = true, message = "تم إضافة الموظف بنجاح", data = employee });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة الموظف" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee == null) return NotFound();

        await LoadEmployeeTypesAsync();
        var model = _mapper.Map<EditEmployeeVM>(employee);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditEmployeeVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var employee = await _service.UpdateAsync(model);
            return Json(new { success = true, message = "تم حفظ التعديلات بنجاح", data = employee });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث الموظف" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int? year, int? month)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var selectedYear = year ?? DateTime.Today.Year;
        var selectedMonth = month ?? DateTime.Today.Month;
        var payroll = (await _attendanceService.GetMonthlyPayrollAsync(selectedYear, selectedMonth, id)).FirstOrDefault();

        ViewBag.SelectedYear = selectedYear;
        ViewBag.SelectedMonth = selectedMonth;
        ViewBag.Payroll = payroll;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", employee);

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return Json(new { success = false, message = "الموظف غير موجود" });
            }

            return Json(new { success = true, message = "تم حذف الموظف بنجاح" });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء حذف الموظف" });
        }
    }

    private async Task LoadEmployeeTypesAsync()
    {
        var employeeTypes = await _employeeTypeService.GetAllAsync();
        ViewBag.EmployeeTypes = new SelectList(employeeTypes, "Id", "Name");
    }

    private static (DateTime? FromDate, DateTime? ToDate) NormalizeDateRange(DateTime? fromDate, DateTime? toDate)
    {
        if (!fromDate.HasValue && !toDate.HasValue)
        {
            var today = DateTime.Today;
            return (today, today);
        }

        fromDate ??= toDate;
        toDate ??= fromDate;

        if (fromDate > toDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        return (fromDate?.Date, toDate?.Date);
    }

    [HttpGet]
    public async Task<IActionResult> Transactions(int id, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            (fromDate, toDate) = NormalizeDateRange(fromDate, toDate);

            var employee = await _service.GetByIdAsync(id);
            if (employee == null)
            {
                TempData["Error"] = "الموظف غير موجود";
                return RedirectToAction(nameof(Index));
            }

            var transactions = await _transactionReportService.GetEmployeeTransactionsByEmployeeIdAsync(id);

            if (fromDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));
            }

            var totalCredits = transactions.Where(t => t.AmountChanged > 0).Sum(t => t.AmountChanged);
            var totalDebits = transactions.Where(t => t.AmountChanged < 0).Sum(t => Math.Abs(t.AmountChanged));

            // Calculate payroll for the selected month (use fromDate or today)
            var selectedYear = fromDate?.Year ?? DateTime.Today.Year;
            var selectedMonth = fromDate?.Month ?? DateTime.Today.Month;
            var payroll = (await _attendanceService.GetMonthlyPayrollAsync(selectedYear, selectedMonth, id)).FirstOrDefault();
            decimal monthNetSalary = payroll?.NetSalary ?? 0m;

            var monthStart = new DateTime(selectedYear, selectedMonth, 1);
            var monthEnd = monthStart.AddMonths(1);
            var paymentsThisMonth = transactions.Where(t => TransactionTypes.IsPayment(t.TransactionType) && t.TransactionDate >= monthStart && t.TransactionDate < monthEnd).Sum(t => Math.Abs(t.AmountChanged));
            var remainingThisMonth = monthNetSalary - paymentsThisMonth;

            ViewBag.Employee = employee;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalCredits = totalCredits; // positive amounts (e.g., accruals)
            ViewBag.TotalDebits = totalDebits;   // negative amounts (payments out)
            ViewBag.CurrentBalance = employee.Balance;
            ViewBag.MonthNetSalary = monthNetSalary; // دائن (what is owed for the month)
            ViewBag.PaymentsThisMonth = paymentsThisMonth; // مدين (what employee took)
            ViewBag.RemainingThisMonth = remainingThisMonth; // positive => still owed, negative => overpaid / advance
            ViewBag.Payroll = payroll;

            return View(transactions.OrderByDescending(t => t.TransactionDate));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل المعاملات";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportTransactions(int id, DateTime? fromDate, DateTime? toDate, string format = "excel")
    {
        try
        {
            (fromDate, toDate) = NormalizeDateRange(fromDate, toDate);

            var employee = await _service.GetByIdAsync(id);
            if (employee == null)
                return NotFound();

            var transactions = await _transactionReportService.GetEmployeeTransactionsByEmployeeIdAsync(id);

            if (fromDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));

            return Json(new
            {
                success = true,
                employee = employee.Name,
                transactions = transactions.OrderByDescending(t => t.TransactionDate)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء التصدير" });
        }
    }
}
