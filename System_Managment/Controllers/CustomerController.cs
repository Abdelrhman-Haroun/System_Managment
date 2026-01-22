using AutoMapper;
using BLL.Services.IService;
using BLL.Services.Service;
using BLL.ViewModels.Customer;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ICustomerService _service;
    private readonly IMapper _mapper;
    private readonly ITransactionReportService _transactionReportService;

    public CustomerController(ICustomerService service, IMapper mapper, ITransactionReportService transactionReportService)
    {
        _service = service;
        _mapper = mapper;
        _transactionReportService = transactionReportService;
    }

    #region Index
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        try
        {
            var customers = await _service.GetAllAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            return View(customers);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
            return View(new List<Customer>());
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
    public async Task<IActionResult> Create(CreateCustomerVM model)
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

            var customer = await _service.CreateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم إضافة العميل بنجاح",
                data = customer
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة العميل" });
        }
    }
    #endregion

    #region Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            var vm = _mapper.Map<EditCustomerVM>(customer);

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
    public async Task<IActionResult> Edit(EditCustomerVM model)
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

            var customer = await _service.UpdateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم الحفظ بنجاح",
                data = customer
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث العميل" });
        }
    }
    #endregion

    #region Details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DetailsPartial", customer);

            return View(customer);
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
                    return Json(new { success = false, message = "العميل غير موجود" });

                TempData["Error"] = "العميل غير موجود";
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
    public async Task<IActionResult> Transactions(int id, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
            {
                TempData["Error"] = "العميل غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // Get all transactions for this customer
            var transactions = await _transactionReportService
                .GetCustomerTransactionsByCustomerIdAsync(id);

            // Apply date filtering if provided
            if (fromDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDate = toDate.Value.AddDays(1);
                transactions = transactions.Where(t => t.TransactionDate < endDate);
            }

            // Calculate summary
            var totalDebt = transactions
                .Where(t => t.TransactionType == "Invoice")
                .Sum(t => t.AmountChanged);

            var totalPayments = transactions
                .Where(t => t.TransactionType == "Payment")
                .Sum(t => Math.Abs(t.AmountChanged));

            ViewBag.Customer = customer;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalDebt = totalDebt;
            ViewBag.TotalPayments = totalPayments;
            ViewBag.CurrentBalance = customer.Balance;

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
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            var transactions = await _transactionReportService
                .GetCustomerTransactionsByCustomerIdAsync(id);

            // Apply date filtering
            if (fromDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));

            // TODO: Implement Excel/PDF export logic
            // For now, return JSON
            return Json(new
            {
                success = true,
                customer = customer.Name,
                transactions = transactions.OrderByDescending(t => t.TransactionDate)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء التصدير" });
        }
    }
}