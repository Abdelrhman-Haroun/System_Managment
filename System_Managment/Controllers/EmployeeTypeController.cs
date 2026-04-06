using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.EmployeeType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers;

[Authorize]
public class EmployeeTypeController : Controller
{
    private readonly IEmployeeTypeService _service;
    private readonly IMapper _mapper;

    public EmployeeTypeController(IEmployeeTypeService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        try
        {
            var items = await _service.GetAllAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            return View(items);
        }
        catch
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل أنواع الموظفين";
            return View(new List<DAL.Models.EmployeeType>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_CreatePartial");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeTypeVM model)
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

            var item = await _service.CreateAsync(model);
            return Json(new { success = true, message = "تم إضافة نوع الموظف بنجاح", data = item });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة نوع الموظف" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();

        var model = _mapper.Map<EditEmployeeTypeVM>(item);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditEmployeeTypeVM model)
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

            var item = await _service.UpdateAsync(model);
            return Json(new { success = true, message = "تم حفظ التعديلات بنجاح", data = item });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث نوع الموظف" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", item);

        return View(item);
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
                return Json(new { success = false, message = "نوع الموظف غير موجود" });
            }

            return Json(new { success = true, message = "تم حذف نوع الموظف بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch
        {
            return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
        }
    }
}
