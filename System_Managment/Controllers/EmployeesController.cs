using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Employee;
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

    public EmployeesController(
        IEmployeeService service,
        IEmployeeTypeService employeeTypeService,
        IEmployeeAttendanceService attendanceService,
        IMapper mapper)
    {
        _service = service;
        _employeeTypeService = employeeTypeService;
        _attendanceService = attendanceService;
        _mapper = mapper;
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
}
