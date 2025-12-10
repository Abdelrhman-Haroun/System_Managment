using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Supplier;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class SupplierController : Controller
{
    #region ctor
    private readonly ISupplierService _service;
    private readonly IMapper _mapper;
    
    public SupplierController(ISupplierService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    #endregion

    #region All
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        var suppliers = await _service.GetAllAsync(s=>!s.IsDeleted);
        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            suppliers = suppliers.Where(u =>
                u.Name.ToLower().Contains(searchTerm) ||
                (u.Phone != null && u.Phone.Contains(searchTerm))
            );
        }
      
        // Order by creation date
        var suppliersList = suppliers.OrderBy(u => u.CreatedAt).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        return View(suppliersList);
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
    public async Task<IActionResult> Create(CreateSupplierVM model)
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

        model.Balance ??= 0;

        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null)
            return Json(new { success = false, message = "هذا المورد موجود بالفعل" });

        var Supplier = _mapper.Map<Supplier>(model);
        await _service.CreateAsync(Supplier);

        return Json(new { success = true, message = "تم إضافة المورد بنجاح" });
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _service.GetByIdAsync(id);
        if (supplier == null)
            return NotFound();

        var vm = _mapper.Map<EditSupplierVM>(supplier);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSupplierVM model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        // Get the existing supplier (tracked by EF)
        var supplier = await _service.GetByIdAsync(model.Id);
        if (supplier == null)
            return Json(new { success = false, message = "المورد غير موجود" });

        // Check duplicate name
        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null && exists.Id != model.Id)
            return Json(new { success = false, message = "هذا الاسم مستخدم من مورد آخر" });

        // Update tracked entity
        supplier.Name = model.Name;
        supplier.Phone = model.Phone;
        supplier.Address = model.Address;

        await _service.UpdateAsync(supplier);

        return Json(new { success = true, message = "تم الحفظ بنجاح" });
    }


    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _service.GetByIdAsync(id);
        if (supplier == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", supplier);

        return View(supplier);
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

