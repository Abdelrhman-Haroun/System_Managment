using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Supplier;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CustomerController : Controller
{
    #region ctor
    private readonly ICustomerService _service;
    private readonly IMapper _mapper;
    
    public CustomerController(ICustomerService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    #endregion

    #region All
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        var Customers = await _service.GetAllAsync(s=>!s.IsDeleted);
        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            Customers = Customers.Where(u =>
                u.Name.ToLower().Contains(searchTerm) ||
                (u.Phone != null && u.Phone.Contains(searchTerm))
            );
        }
      
        // Order by creation date
        var CustomersList = Customers.OrderBy(u => u.CreatedAt).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        return View(CustomersList);
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
    public async Task<IActionResult> Create(CreateVM model)
    {
        if (ModelState.IsValid)
        {
            // Default balance = 0 if empty
            model.Balance ??= 0;

            // Check duplicate name
            var exists = await _service.GetByNameAsync(model.Name);
            if (exists != null)
            {
                ModelState.AddModelError("Name", "هذا المورد موجود بالفعل");
                return View(model);
            }

            var Customer = _mapper.Map<Customer>(model);

            await _service.CreateAsync(Customer);
            TempData["SuccessMessage"] = "تم إضافة المورد بنجاح";
        }
        return View(model);
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var Customer = await _service.GetByIdAsync(id);
        if (Customer == null)
            return NotFound();

        var vm = _mapper.Map<EditVM>(Customer);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditVM model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        // Get the existing Customer (tracked by EF)
        var Customer = await _service.GetByIdAsync(model.Id);
        if (Customer == null)
            return Json(new { success = false, message = "المورد غير موجود" });

        // Check duplicate name
        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null && exists.Id != model.Id)
            return Json(new { success = false, message = "هذا الاسم مستخدم من مورد آخر" });

        // Update tracked entity
        Customer.Name = model.Name;
        Customer.Phone = model.Phone;
        Customer.Address = model.Address;

        await _service.UpdateAsync(Customer);

        return Json(new { success = true, message = "تم الحفظ بنجاح" });
    }


    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var Customer = await _service.GetByIdAsync(id);
        if (Customer == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", Customer);

        return View(Customer);
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

