using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class ProductController : Controller
{
    #region ctor
    private readonly IProductService _service;
    private readonly IStoreService _storeService;
    private readonly IProductCategoryService _produictCategoryService;
    private readonly IMapper _mapper;

    public ProductController(IProductService service, IProductCategoryService catService, IStoreService storeService, IMapper mapper)
    {
        _service = service;
        _produictCategoryService = catService;
        _storeService = storeService;
        _mapper = mapper;
    }
    #endregion

    #region All
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        var products = await _service.GetAllAsync(p => !p.IsDeleted, "Category,Store");
        var categories = await _produictCategoryService.GetAllAsync(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();

            // Get category IDs that match the search term
            var matchedCategoryIds = categories
                .Where(c => c.Name.ToLower().Contains(searchTerm))
                .Select(c => c.Id)
                .ToList();

            // Filter products by name or category ID
            products = products
                .Where(p => p.Name.ToLower().Contains(searchTerm)|| matchedCategoryIds.Contains(p.CategoryId));
        }


        // Order by creation date
        var ProductsList = products.OrderBy(u => u.CreatedAt).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        return View(ProductsList);
    }

    #endregion

    #region Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Stores = new SelectList(await _storeService.GetAllAsync(s=>!s.IsDeleted), "Id", "Name");
        ViewBag.Categories = new SelectList(await _produictCategoryService.GetAllAsync(s => !s.IsDeleted), "Id", "Name");

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductVM model)
    {
        if (ModelState.IsValid)
        {
            // Check duplicate name
            var exists = await _service.GetByNameAsync(model.Name);
            if (exists != null)
            {
                ModelState.AddModelError("Name", "هذا المنتج موجود بالفعل");
                return View(model);
            }

            var Product = _mapper.Map<Product>(model);

            await _service.CreateAsync(Product);
            TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
        }

        // reload dropdowns
        ViewBag.Stores = new SelectList(await _storeService.GetAllAsync(s => !s.IsDeleted), "Id", "Name");
        ViewBag.Categories = new SelectList(await _produictCategoryService.GetAllAsync(s => !s.IsDeleted), "Id", "Name");
        return View(model);
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var Product = await _service.GetByIdAsync(id);
        if (Product == null)
            return NotFound();

        var vm = _mapper.Map<EditProductVM>(Product);

        ViewBag.Stores = new SelectList(await _storeService.GetAllAsync(s => !s.IsDeleted), "Id", "Name");
        ViewBag.Categories = new SelectList(await _produictCategoryService.GetAllAsync(s => !s.IsDeleted), "Id", "Name");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", vm);

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductVM model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        // Get the existing Product (tracked by EF)
        var Product = await _service.GetByIdAsync(model.Id);
        if (Product == null)
            return Json(new { success = false, message = "المنتج غير موجود" });

        // Check duplicate name
        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null && exists.Id != model.Id)
            return Json(new { success = false, message = "هذا الاسم مستخدم من منتج آخر" });

        // Update tracked entity
        Product.Name = model.Name;
        Product.Description = model.Description;
        Product.StoreId = model.StoreId;
        Product.CategoryId = model.CategoryId;


        await _service.UpdateAsync(Product);

        return Json(new { success = true, message = "تم الحفظ بنجاح" });
    }


    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var Product = await _service.GetByIdAsync(id);
        if (Product == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", Product);

        return View(Product);
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

