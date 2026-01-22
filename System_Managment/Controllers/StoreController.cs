using AutoMapper;
using BLL.Services.IService;
using BLL.Services.Service;
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
    private readonly IProductService _productService;
    private readonly ITransactionReportService _transactionReportService;

    public StoreController(IStoreService service, IMapper mapper, ITransactionReportService transactionReportService, IProductService productService)
    {
        _service = service;
        _mapper = mapper;
        _transactionReportService = transactionReportService;
        _productService = productService;
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


    [HttpGet]
    public async Task<IActionResult> Inventory(int id, DateTime? fromDate, DateTime? toDate, string searchTerm = null)
    {
        try
        {
            var store = await _service.GetByIdAsync(id);
            if (store == null)
            {
                TempData["Error"] = "المخزن غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // Get all products in this store
            var products = await _productService.GetAllAsync(
                p => p.StoreId == id && !p.IsDeleted,
                "Category");

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(term))
                );
            }

            // Calculate stock value for each product
            var inventoryItems = new List<StoreInventoryItemVM>();

            foreach (var product in products)
            {
                // Get transactions for this product within date range
                var transactions = await _transactionReportService
                    .GetProductTransactionsByProductIdAsync(product.Id);

                if (fromDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);

                if (toDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));

                var transactionsList = transactions.ToList();

                var totalIn = transactionsList
                    .Where(t => t.TransactionType == "Purchase")
                    .Sum(t => t.QuantityChanged);

                var totalOut = transactionsList
                    .Where(t => t.TransactionType == "Sales")
                    .Sum(t => Math.Abs(t.QuantityChanged));

                var avgPurchasePrice = transactionsList
                    .Where(t => t.TransactionType == "Purchase" && t.UnitPrice > 0)
                    .Average(t => (decimal?)t.UnitPrice) ?? 0;

                var lastTransaction = transactionsList.FirstOrDefault();

                inventoryItems.Add(new StoreInventoryItemVM
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductType = product.ProductType,
                    CategoryName = product.Category?.Name ?? "غير محدد",
                    CurrentStock = product.StockQuantity ?? 0,
                    TotalIn = totalIn,
                    TotalOut = totalOut,
                    AveragePurchasePrice = avgPurchasePrice,
                    StockValue = (product.StockQuantity ?? 0) * avgPurchasePrice,
                    LastTransactionDate = lastTransaction?.TransactionDate,
                    LastUnitPrice = lastTransaction?.UnitPrice ?? 0
                });
            }

            ViewBag.Store = store;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TotalStockValue = inventoryItems.Sum(i => i.StockValue);
            ViewBag.TotalProducts = inventoryItems.Count;
            ViewBag.LowStockProducts = inventoryItems.Count(i => i.CurrentStock < 10 && i.ProductType ==1 || i.CurrentStock < 1000 && i.ProductType == 2);

            return View(inventoryItems.OrderBy(i => i.ProductName));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل تقرير المخزون";
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportInventory(int id, DateTime? fromDate, DateTime? toDate, string format = "excel")
    {
        try
        {
            var store = await _service.GetByIdAsync(id);
            if (store == null)
                return NotFound();

            // TODO: Implement Excel/PDF export logic
            return Json(new { success = true, message = "سيتم إضافة التصدير قريباً" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء التصدير" });
        }
    }
}