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
            (startDate, endDate) = NormalizeDateRange(startDate, endDate);

            IEnumerable<InternalUsageDetailsVM> usages;

            if (!string.IsNullOrEmpty(category))
                usages = await _usageService.GetUsageByCategoryAsync(category);
            else
                usages = await _usageService.GetUsageByDateRangeAsync(startDate.Value, endDate.Value);

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

            return View(new CreateInternalUsageVM
            {
                UsageDate = DateTime.Today
            });
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

        [HttpGet]
        public async Task<IActionResult> DetailsByReference(int productId, string referenceNumber)
        {
            if (productId <= 0 || string.IsNullOrWhiteSpace(referenceNumber))
                return NotFound();

            var usages = await _usageService.GetAllInternalUsageAsync();
            var usage = usages.FirstOrDefault(u =>
                u.ProductId == productId &&
                string.Equals(u.ReferenceNumber, referenceNumber, StringComparison.OrdinalIgnoreCase));

            if (usage == null)
                return NotFound();

            return RedirectToAction(nameof(Details), new { id = usage.Id });
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

        private static (DateTime? StartDate, DateTime? EndDate) NormalizeDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue && !endDate.HasValue)
            {
                var today = DateTime.Today;
                return (today, today);
            }

            startDate ??= endDate;
            endDate ??= startDate;

            if (startDate > endDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            return (startDate?.Date, endDate?.Date);
        }
    }
}
