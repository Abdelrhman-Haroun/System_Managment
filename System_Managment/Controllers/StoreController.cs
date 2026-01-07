using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Store;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers;

[Authorize]
public class StoreController : Controller
{
    private readonly IStoreService _service;
    private readonly IMapper _mapper;

    public StoreController(IStoreService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    #region Index
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        try
        {
            var Stores = await _service.GetAllAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            return View(Stores);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
            return View(new List<Store>());
        }
    }
    #endregion

    #region Create
    [HttpGet]
    public IActionResult Create()
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_CreatePartial");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStoreVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = string.Join(", ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var Store = await _service.CreateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم إضافة المخزن بنجاح",
                data = Store
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة المخزن" });
        }
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var Store = await _service.GetByIdAsync(id);
            if (Store == null)
                return NotFound();

            var vm = _mapper.Map<EditStoreVM>(Store);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditPartial", vm);

            return View(vm);
        }
        catch (Exception ex)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStoreVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "بيانات غير صحيحة: " + string.Join(", ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            var Store = await _service.UpdateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم الحفظ بنجاح",
                data = Store
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث المخزن" });
        }
    }
    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var Store = await _service.GetByIdAsync(id);
            if (Store == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DetailsPartial", Store);

            return View(Store);
        }
        catch (Exception ex)
        {
            return NotFound();
        }
    }
    #endregion

    #region Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "المخزن غير موجود" });

                TempData["Error"] = "المخزن غير موجود";
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "تم الحذف بنجاح" });

            TempData["Success"] = "تم الحذف بنجاح";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });

            TempData["Error"] = "حدث خطأ أثناء الحذف";
            return RedirectToAction(nameof(Index));
        }
    }
    #endregion
}