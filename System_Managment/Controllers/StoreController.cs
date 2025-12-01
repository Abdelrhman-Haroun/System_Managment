using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Store;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class StoreController : Controller
{
    #region ctor
    private readonly IStoreService _service;
    private readonly IMapper _mapper;
    
    public StoreController(IStoreService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    #endregion

    #region All
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        var Stores = await _service.GetAllAsync(s=>!s.IsDeleted);
        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            Stores = Stores.Where(u =>
                u.Name.ToLower().Contains(searchTerm) 
            );
        }
      
        // Order by creation date
        var StoresList = Stores.OrderBy(u => u.CreatedAt).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        return View(StoresList);
    }
    #endregion

    #region Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStoreVM model)
    {
        if (ModelState.IsValid)
        {
            // Check duplicate name
            var exists = await _service.GetByNameAsync(model.Name);
            if (exists != null)
            {
                ModelState.AddModelError("Name", "هذة الفئة موجودة بالفعل");
                return View(model);
            }

            var Store = _mapper.Map<Store>(model);

            await _service.CreateAsync(Store);
            TempData["SuccessMessage"] = "تم إضافة المخزن بنجاح";
        }
        return View(model);
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var Store = await _service.GetByIdAsync(id);
        if (Store == null)
            return NotFound();

        var vm = _mapper.Map<EditStoreVM>(Store);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStoreVM model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        // Get the existing Store (tracked by EF)
        var Store = await _service.GetByIdAsync(model.Id);
        if (Store == null)
            return Json(new { success = false, message = "المخزن غير موجودة" });

        // Check duplicate name
        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null && exists.Id != model.Id)
            return Json(new { success = false, message = "هذا الاسم مستخدم من قبل" });

        // Update tracked entity
        Store.Name = model.Name;
        Store.Description = model.Description;
     
        await _service.UpdateAsync(Store);

        return Json(new { success = true, message = "تم الحفظ بنجاح" });
    }


    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var Store = await _service.GetByIdAsync(id);
        if (Store == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", Store);

        return View(Store);
    }
    #endregion

    #region Delete
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true });

        return RedirectToAction(nameof(Index));
    }
    #endregion
}

