using AutoMapper;
using BLL.Services.IService;
using BLL.Services.Service;
using BLL.ViewModels.Supplier;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System_Managment.Controllers;

[Authorize]
public class SupplierController : Controller
{
    private readonly ISupplierService _service;
    private readonly IMapper _mapper;
    private readonly ITransactionReportService _transactionReportService;

    public SupplierController(ISupplierService service, IMapper mapper, ITransactionReportService transactionReportService)
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
            var Suppliers = await _service.GetAllAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            return View(Suppliers);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
            return View(new List<Supplier>());
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
    public async Task<IActionResult> Create(CreateSupplierVM model)
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

            var Supplier = await _service.CreateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم إضافة العميل بنجاح",
                data = Supplier
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
            var Supplier = await _service.GetByIdAsync(id);
            if (Supplier == null)
                return NotFound();

            var vm = _mapper.Map<EditSupplierVM>(Supplier);

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
    public async Task<IActionResult> Edit(EditSupplierVM model)
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

            var Supplier = await _service.UpdateAsync(model);

            return Json(new
            {
                success = true,
                message = "تم الحفظ بنجاح",
                data = Supplier
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
            var Supplier = await _service.GetByIdAsync(id);
            if (Supplier == null)
                return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DetailsPartial", Supplier);

            return View(Supplier);
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
            var supplier = await _service.GetByIdAsync(id);
            if (supplier == null)
            {
                TempData["Error"] = "المورد غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // Get all transactions for this supplier
            var transactions = await _transactionReportService
                .GetSupplierTransactionsBySupplierIdAsync(id);

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
            var totalPurchases = transactions
                .Where(t => t.TransactionType == "Purchase")
                .Sum(t => t.AmountChanged);

            var totalPayments = transactions
                .Where(t => t.TransactionType == "Payment")
                .Sum(t => Math.Abs(t.AmountChanged));

            ViewBag.Supplier = supplier;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.TotalPayments = totalPayments;
            ViewBag.CurrentBalance = supplier.Balance;

            return View(transactions.OrderByDescending(t => t.TransactionDate));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "حدث خطأ أثناء تحميل المعاملات";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Export supplier transactions to Excel or PDF
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportTransactions(int id, DateTime? fromDate, DateTime? toDate, string format = "excel")
    {
        try
        {
            var supplier = await _service.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();

            var transactions = await _transactionReportService
                .GetSupplierTransactionsBySupplierIdAsync(id);

            // Apply date filtering
            if (fromDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                transactions = transactions.Where(t => t.TransactionDate < toDate.Value.AddDays(1));

            // TODO: Implement Excel/PDF export logic
            return Json(new
            {
                success = true,
                supplier = supplier.Name,
                transactions = transactions.OrderByDescending(t => t.TransactionDate)
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "حدث خطأ أثناء التصدير" });
        }
    }
}