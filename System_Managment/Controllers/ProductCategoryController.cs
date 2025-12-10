using AutoMapper;
using BLL.Services.IService;
using BLL.ViewModels.ProductCategory;
using BLL.ViewModels.ProductCategory;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ProductCategoryController : Controller
{
    #region ctor
    private readonly IProductCategoryService _service;
    private readonly IMapper _mapper;
    
    public ProductCategoryController(IProductCategoryService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    #endregion

    #region All
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        var ProductCategorys = await _service.GetAllAsync(s=>!s.IsDeleted);
        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            ProductCategorys = ProductCategorys.Where(u =>
                u.Name.ToLower().Contains(searchTerm) 
            );
        }
      
        // Order by creation date
        var ProductCategorysList = ProductCategorys.OrderBy(u => u.CreatedAt).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        return View(ProductCategorysList);
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
    public async Task<IActionResult> Create(CreateProductCategoryVM model)
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

        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null)
            return Json(new { success = false, message = "هذة الفئة موجودة بالفعل" });

        var ProductCategory = _mapper.Map<ProductCategory>(model);
        await _service.CreateAsync(ProductCategory);

        return Json(new { success = true, message = "تم إضافة الفئة بنجاح" });
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ProductCategory = await _service.GetByIdAsync(id);
        if (ProductCategory == null)
            return NotFound();

        var vm = _mapper.Map<EditProductCategoryVM>(ProductCategory);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_EditPartial", vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProductCategoryVM model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "بيانات غير صحيحة: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        // Get the existing ProductCategory (tracked by EF)
        var ProductCategory = await _service.GetByIdAsync(model.Id);
        if (ProductCategory == null)
            return Json(new { success = false, message = "فئة المنتج غير موجودة" });

        // Check duplicate name
        var exists = await _service.GetByNameAsync(model.Name);
        if (exists != null && exists.Id != model.Id)
            return Json(new { success = false, message = "هذا الاسم مستخدم من قبل" });

        // Update tracked entity
        ProductCategory.Name = model.Name;
        ProductCategory.Description = model.Description;
     
        await _service.UpdateAsync(ProductCategory);

        return Json(new { success = true, message = "تم الحفظ بنجاح" });
    }


    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ProductCategory = await _service.GetByIdAsync(id);
        if (ProductCategory == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_DetailsPartial", ProductCategory);

        return View(ProductCategory);
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

