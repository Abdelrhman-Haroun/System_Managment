using AutoMapper;
using BLL.Services.IService;
using BLL.Services.Service;
using BLL.ViewModels.Product;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace System_Managment.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly IStoreService _storeService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly ITransactionReportService _transactionReportService;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService service,
            IProductCategoryService categoryService,
            IStoreService storeService,
            IMapper mapper,
            ITransactionReportService transactionReportService)
        {
            _service = service;
            _productCategoryService = categoryService;
            _storeService = storeService;
            _mapper = mapper;
            _transactionReportService = transactionReportService;
        }

        #region Index
        public async Task<IActionResult> Index(string searchTerm, int page = 1)
        {
            try
            {
                // Get all products with related data (Category, Store)
                var products = await _service.GetAllAsync(searchTerm, "Category,Store");

                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return View(new List<Product>());
            }
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadSelectLists();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_CreatePartial");

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductVM model)
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

                var product = await _service.CreateAsync(model);

                return Json(new
                {
                    success = true,
                    message = "تم إضافة المنتج بنجاح",
                    data = product
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء إضافة المنتج" });
            }
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _service.GetByIdAsync(id);
                if (product == null)
                    return NotFound();

                var vm = _mapper.Map<EditProductVM>(product);

                await LoadSelectLists();

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
        public async Task<IActionResult> Edit(EditProductVM model)
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

                var product = await _service.UpdateAsync(model);

                return Json(new
                {
                    success = true,
                    message = "تم الحفظ بنجاح",
                    data = product
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث المنتج" });
            }
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _service.GetByIdContainsAsync(id, "Category,Store");
                if (product == null)
                    return NotFound();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_DetailsPartial", product);

                return View(product);
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
                        return Json(new { success = false, message = "المنتج غير موجود" });

                    TempData["Error"] = "المنتج غير موجود";
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

        #region Helper Methods
        private async Task LoadSelectLists()
        {
            var stores = await _storeService.GetAllAsync(s => !s.IsDeleted);
            var categories = await _productCategoryService.GetAllAsync(c => !c.IsDeleted);

            ViewBag.Stores = new SelectList(stores, "Id", "Name");
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> Transactions(int id, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var product = await _service.GetByIdContainsAsync(id, "Category,Store");
                if (product == null)
                {
                    TempData["Error"] = "المنتج غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                // Get all transactions for this product (includes internal usage)
                var transactions = await _transactionReportService
                    .GetProductTransactionsByProductIdAsync(id);

                // Apply date filtering if provided
                if (fromDate.HasValue)
                {
                    transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.AddDays(1);
                    transactions = transactions.Where(t => t.TransactionDate < endDate);
                }

                // Calculate summary including internal usage
                var totalPurchases = transactions
                    .Where(t => TransactionTypes.IsPurchase(t.TransactionType))
                    .Sum(t => t.QuantityChanged);

                var totalSales = transactions
                    .Where(t => TransactionTypes.IsSales(t.TransactionType))
                    .Sum(t => Math.Abs(t.QuantityChanged));

                // NEW: Calculate internal usage separately
                var totalInternalUsage = transactions
                    .Where(t => TransactionTypes.IsInternalUsage(t.TransactionType))
                    .Sum(t => Math.Abs(t.QuantityChanged));

                var totalPurchaseValue = transactions
                    .Where(t => TransactionTypes.IsPurchase(t.TransactionType))
                    .Sum(t => t.TotalAmount);

                var totalSalesValue = transactions
                    .Where(t => TransactionTypes.IsSales(t.TransactionType))
                    .Sum(t => t.TotalAmount);

                // NEW: Calculate internal usage value
                var totalInternalUsageValue = transactions
                    .Where(t => TransactionTypes.IsInternalUsage(t.TransactionType))
                    .Sum(t => t.TotalAmount);

                ViewBag.Product = product;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.TotalPurchases = totalPurchases;
                ViewBag.TotalSales = totalSales;
                ViewBag.TotalInternalUsage = totalInternalUsage; // NEW
                ViewBag.TotalPurchaseValue = totalPurchaseValue;
                ViewBag.TotalSalesValue = totalSalesValue;
                ViewBag.TotalInternalUsageValue = totalInternalUsageValue; // NEW
                ViewBag.CurrentStock = product.StockQuantity;
                ViewBag.ProductType = product.ProductType;

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
                var product = await _service.GetByIdAsync(id);
                if (product == null)
                    return NotFound();

                var transactions = await _transactionReportService
                    .GetProductTransactionsByProductIdAsync(id);

                // Apply date filtering
                if (fromDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);

                if (toDate.HasValue)
                    transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));

                // TODO: Implement Excel/PDF export logic
                return Json(new
                {
                    success = true,
                    product = product.Name,
                    transactions = transactions.OrderByDescending(t => t.TransactionDate)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء التصدير" });
            }
        }
    }
}
