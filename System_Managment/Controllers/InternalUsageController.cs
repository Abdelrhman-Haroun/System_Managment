using BLL.Services.IService;
using BLL.ViewModels.InternalUsage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers
{
    [Authorize]
    public class InternalUsageController : Controller
    {
        private readonly IInternalProductUsageService _usageService;
        private readonly IProductService _productService;

        public InternalUsageController(
            IInternalProductUsageService usageService,
            IProductService productService)
        {
            _usageService = usageService;
            _productService = productService;
        }

        // INDEX - Show all usage records
        public async Task<IActionResult> Index(
            string category = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IEnumerable<InternalUsageDetailsVM> usages;

            if (!string.IsNullOrEmpty(category))
                usages = await _usageService.GetUsageByCategoryAsync(category);
            else if (startDate.HasValue && endDate.HasValue)
                usages = await _usageService.GetUsageByDateRangeAsync(startDate.Value, endDate.Value);
            else
                usages = await _usageService.GetAllInternalUsageAsync();

            ViewBag.SelectedCategory = category;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(usages.ToList());
        }

        // CREATE GET - Show form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await _productService.GetAllAsync();
            ViewBag.Products = products.Where(p => p.StockQuantity > 0 && !p.IsDeleted).ToList();
            ViewBag.Categories = new List<string>
            {
                "تغذية الحيوانات",
                "عمليات المزرعة",
                "العناية بالماشية",
                "الصيانة",
                "أخرى"
            };

            return View();
        }

        // CREATE POST - Process form (AJAX Support)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInternalUsageVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Check if AJAX request
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "يرجى التحقق من البيانات المدخلة",
                            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                        });
                    }

                    var products = await _productService.GetAllAsync();
                    ViewBag.Products = products.Where(p => p.StockQuantity > 0 && !p.IsDeleted).ToList();
                    ViewBag.Categories = new List<string>
                    {
                        "تغذية الحيوانات",
                        "عمليات المزرعة",
                        "العناية بالماشية",
                        "الصيانة",
                        "أخرى"
                    };
                    return View(model);
                }

                var result = await _usageService.RecordInternalUsageAsync(model);

                // Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    if (result.Success)
                    {
                        return Json(new
                        {
                            success = true,
                            message = result.Message,
                            invoiceId = result.UsageId,
                            redirectUrl = Url.Action(nameof(Details), new { id = result.UsageId })
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }
                }

                // Regular form submission
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    var products = await _productService.GetAllAsync();
                    ViewBag.Products = products.Where(p => p.StockQuantity > 0 && !p.IsDeleted).ToList();
                    ViewBag.Categories = new List<string>
                    {
                        "تغذية الحيوانات",
                        "عمليات المزرعة",
                        "العناية بالماشية",
                        "الصيانة",
                        "أخرى"
                    };
                    return View(model);
                }

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.UsageId });
            }
            catch (Exception ex)
            {
                // If AJAX, return JSON error
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"حدث خطأ غير متوقع: {ex.Message}"
                    });
                }

                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }



        // EDIT GET - Show form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usage = await _usageService.GetUsageDetailsAsync(id);
            if (usage == null)
                return NotFound();

            var model = new UpdateInternalUsageVM
            {
                Id = usage.Id,
                ProductId = usage.ProductId,
                Quantity = usage.Quantity,
                Weight = usage.Weight,
                UnitPrice = usage.UnitPrice,
                UsageCategory = usage.UsageCategory,
                UsageDate = usage.UsageDate,
                Notes = usage.Notes
            };

            var products = await _productService.GetAllAsync();
            ViewBag.Products = products.Where(p => !p.IsDeleted).ToList();
            ViewBag.Categories = new List<string>
            {
                "تغذية الحيوانات",
                "عمليات المزرعة",
                "العناية بالماشية",
                "الصيانة",
                "أخرى"
            };

            return View(model);
        }

        // EDIT POST - Process form (AJAX Support)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateInternalUsageVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Always check if it's an AJAX request (from jQuery)
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "يرجى التحقق من البيانات المدخلة",
                            errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                        });
                    }

                    var products = await _productService.GetAllAsync();
                    ViewBag.Products = products.Where(p => !p.IsDeleted).ToList();
                    ViewBag.Categories = new List<string>
                    {
                        "تغذية الحيوانات",
                        "عمليات المزرعة",
                        "العناية بالماشية",
                        "الصيانة",
                        "أخرى"
                    };
                    return View(model);
                }

                var result = await _usageService.UpdateInternalUsageAsync(model);

                // Check if AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    if (result.Success)
                    {
                        return Json(new
                        {
                            success = true,
                            message = result.Message,
                            invoiceId = model.Id,
                            redirectUrl = Url.Action(nameof(Details), new { id = model.Id })
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }
                }

                // Regular form submission
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    var products = await _productService.GetAllAsync();
                    ViewBag.Products = products.Where(p => !p.IsDeleted).ToList();
                    ViewBag.Categories = new List<string>
                    {
                        "تغذية الحيوانات",
                        "عمليات المزرعة",
                        "العناية بالماشية",
                        "الصيانة",
                        "أخرى"
                    };
                    return View(model);
                }

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                // If AJAX, return JSON error
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.ContentType?.Contains("application/json") == true)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"حدث خطأ غير متوقع: {ex.Message}"
                    });
                }

                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
        }

        // DETAILS - Show single record
        public async Task<IActionResult> Details(int id)
        {
            var usage = await _usageService.GetUsageDetailsAsync(id);
            if (usage == null)
                return NotFound();

            return View(usage);
        }

        // DELETE - Soft delete record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _usageService.DeleteInternalUsageAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // BY PRODUCT - Show usage for specific product
        public async Task<IActionResult> ByProduct(int productId)
        {
            var usages = await _usageService.GetUsageByProductAsync(productId);
            var summary = await _usageService.GetProductUsageSummaryAsync(productId);
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return NotFound();

            ViewBag.Product = product;
            ViewBag.TotalCost = summary.TotalCost;
            ViewBag.TotalQuantity = summary.TotalQuantity;

            return View(usages.ToList());
        }
    }
}