using BLL.Services.IService;
using BLL.ViewModels.Invoice;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ISupplierService _supplierService;
        private readonly ITransactionReportService _transactionReportService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceService invoiceService,
            IProductService productService,
            ICustomerService customerService,
            ISupplierService supplierService,
            ITransactionReportService transactionReportService,
            ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _productService = productService;
            _customerService = customerService;
            _supplierService = supplierService;
            _transactionReportService = transactionReportService;
            _logger = logger;
        }

        // GET: Invoices
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? type)
        {
            try
            {
                var invoices = await _invoiceService.GetAllInvoicesAsync(type);
                ViewBag.SelectedType = type;
                return View(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل الفواتير";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Invoices/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadDropdownData();
                return View(new CreateInvoiceVM());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create GET: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل الصفحة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Invoices/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvoiceVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for invoice creation");
                    return Json(new
                    {
                        success = false,
                        message = "يرجى التحقق من البيانات المدخلة",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var (success, message, invoiceId) = await _invoiceService.CreateInvoiceAsync(model);

                if (success)
                {
                    _logger.LogInformation($"Invoice created successfully: {invoiceId}");

                    return Json(new
                    {
                        success = true,
                        message = message,
                        invoiceId = invoiceId,
                        redirectUrl = Url.Action("Details", new { id = invoiceId })
                    });
                }

                _logger.LogWarning($"Invoice creation failed: {message}");
                return Json(new { success = false, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Create POST: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع أثناء إنشاء الفاتورة"
                });
            }
        }

        // GET: Invoices/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "معرف الفاتورة غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);

                if (invoice == null)
                {
                    TempData["Error"] = "الفاتورة غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                // Load transaction history if you want to display it
                var productTransactions = await _transactionReportService
                    .GetProductTransactionsByInvoiceIdAsync(id);

                ViewBag.ProductTransactions = productTransactions;

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Details: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل تفاصيل الفاتورة";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Invoices/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "معرف الفاتورة غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);

                if (invoice == null)
                {
                    TempData["Error"] = "الفاتورة غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateInvoiceVM
                {
                    Id = invoice.Id,
                    InvoiceType = invoice.InvoiceType,
                    CustomerId = invoice.CustomerId,
                    SupplierId = invoice.SupplierId,
                    ReferenceNumber = invoice.ReferenceNumber,
                    DiscountAmount = invoice.DiscountAmount,
                    TaxAmount = invoice.TaxAmount,
                    Notes = invoice.Notes,
                    Items = invoice.Items.Select(i => new InvoiceItemVM
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Weight = i.Weight,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                await LoadDropdownData();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Edit GET: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل الصفحة";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Invoices/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateInvoiceVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state for invoice update: {model.Id}");
                    return Json(new
                    {
                        success = false,
                        message = "يرجى التحقق من البيانات المدخلة",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var (success, message) = await _invoiceService.UpdateInvoiceAsync(model);

                if (success)
                {
                    _logger.LogInformation($"Invoice updated successfully: {model.Id}");

                    return Json(new
                    {
                        success = true,
                        message = message,
                        invoiceId = model.Id,
                        redirectUrl = Url.Action("Details", new { id = model.Id })
                    });
                }

                _logger.LogWarning($"Invoice update failed: {message}");
                return Json(new { success = false, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Edit POST: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع أثناء تحديث الفاتورة"
                });
            }
        }

        // POST: Invoices/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "معرف الفاتورة غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var (success, message) = await _invoiceService.DeleteInvoiceAsync(id);

                if (success)
                {
                    _logger.LogInformation($"Invoice deleted successfully: {id}");
                    TempData["Success"] = message;
                }
                else
                {
                    _logger.LogWarning($"Invoice deletion failed: {message}");
                    TempData["Error"] = message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in Delete: {ex.Message}");
                TempData["Error"] = "حدث خطأ غير متوقع أثناء حذف الفاتورة";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== NEW: Transaction History Endpoints ==========

        // GET: Invoices/TransactionHistory/5
        [HttpGet("TransactionHistory/{id}")]
        public async Task<IActionResult> TransactionHistory(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "معرف الفاتورة غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);
                if (invoice == null)
                {
                    TempData["Error"] = "الفاتورة غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                // Get all transactions related to this invoice
                var productTransactions = await _transactionReportService
                    .GetProductTransactionsByInvoiceIdAsync(id);

                var customerTransactions = await _transactionReportService
                    .GetCustomerTransactionsByInvoiceIdAsync(id);

                var supplierTransactions = await _transactionReportService
                    .GetSupplierTransactionsByInvoiceIdAsync(id);

                ViewBag.Invoice = invoice;
                ViewBag.ProductTransactions = productTransactions;
                ViewBag.CustomerTransactions = customerTransactions;
                ViewBag.SupplierTransactions = supplierTransactions;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in TransactionHistory: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل سجل المعاملات";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Invoices/ProductTransactions/5
        [HttpGet("ProductTransactions/{productId}")]
        public async Task<IActionResult> ProductTransactionHistory(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return Json(new { success = false, message = "معرف المنتج غير صحيح" });
                }

                var product = await _productService.GetByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "المنتج غير موجود" });
                }

                var transactions = await _transactionReportService
                    .GetProductTransactionsByProductIdAsync(productId);

                return Json(new { success = true, data = transactions });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProductTransactionHistory: {ex.Message}");
                return Json(new { success = false, message = "حدث خطأ في تحميل السجل" });
            }
        }

        // GET: Invoices/CustomerTransactions/5
        [HttpGet("CustomerTransactions/{customerId}")]
        public async Task<IActionResult> CustomerTransactionHistory(int customerId)
        {
            try
            {
                if (customerId <= 0)
                {
                    return Json(new { success = false, message = "معرف العميل غير صحيح" });
                }

                var customer = await _customerService.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "العميل غير موجود" });
                }

                var transactions = await _transactionReportService
                    .GetCustomerTransactionsByCustomerIdAsync(customerId);

                var summary = await _transactionReportService
                    .GetCustomerDebtSummaryAsync(customerId);

                return Json(new
                {
                    success = true,
                    data = transactions,
                    summary = new { totalDebt = summary.TotalDebt, invoiceCount = summary.InvoiceCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CustomerTransactionHistory: {ex.Message}");
                return Json(new { success = false, message = "حدث خطأ في تحميل السجل" });
            }
        }

        // GET: Invoices/SupplierTransactions/5
        [HttpGet("SupplierTransactions/{supplierId}")]
        public async Task<IActionResult> SupplierTransactionHistory(int supplierId)
        {
            try
            {
                if (supplierId <= 0)
                {
                    return Json(new { success = false, message = "معرف المورد غير صحيح" });
                }

                var supplier = await _supplierService.GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return Json(new { success = false, message = "المورد غير موجود" });
                }

                var transactions = await _transactionReportService
                    .GetSupplierTransactionsBySupplierIdAsync(supplierId);

                var summary = await _transactionReportService
                    .GetSupplierDebtSummaryAsync(supplierId);

                return Json(new
                {
                    success = true,
                    data = transactions,
                    summary = new { totalCredit = summary.TotalCredit, invoiceCount = summary.InvoiceCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SupplierTransactionHistory: {ex.Message}");
                return Json(new { success = false, message = "حدث خطأ في تحميل السجل" });
            }
        }

        // ========== Helper Methods ==========

        private async Task LoadDropdownData()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync(s => !s.IsDeleted);
                var customers = await _customerService.GetAllAsync(c => !c.IsDeleted);
                var products = await _productService.GetAllAsync(p => !p.IsDeleted);

                ViewBag.Suppliers = suppliers?.ToList() ?? new List<Supplier>();
                ViewBag.Customers = customers?.ToList() ?? new List<Customer>();
                ViewBag.Products = products?.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.StockQuantity,
                    p.ProductType
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dropdown data: {ex.Message}");
                ViewBag.Suppliers = new List<Supplier>();
                ViewBag.Customers = new List<Customer>();
                ViewBag.Products = new List<object>();
            }
        }

        [HttpGet("Print/{id}")]
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "معرف الفاتورة غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var invoice = await _invoiceService.GetInvoiceDetailsAsync(id);

                if (invoice == null)
                {
                    TempData["Error"] = "الفاتورة غير موجودة";
                    return RedirectToAction(nameof(Index));
                }

                // Get company information (you might want to store this in a settings table)
                ViewBag.CompanyName = "مزرعة أل سالم";
                ViewBag.CompanyAddress = "الخطاطبة، الطريق الاقليمى";
                ViewBag.CompanyPhone = "+20 123 456 7890";
                ViewBag.CompanyEmail = "info@company.com";
                ViewBag.CompanyTaxNumber = "123-456-789";

                // Optional: Logo path (store in wwwroot/images/logo.png)
                ViewBag.LogoPath = "/Files/images/logo.jpg";

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Print: {ex.Message}");
                TempData["Error"] = "حدث خطأ في تحميل الفاتورة للطباعة";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}