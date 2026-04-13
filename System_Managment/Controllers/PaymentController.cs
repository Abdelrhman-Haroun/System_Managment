using BLL.Services.IService;
using BLL.ViewModels.Payment;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace System_Managment.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(string? searchTerm, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            var model = await _paymentService.GetAllAsync(searchTerm);
            var payments = model.Payments.ToList();

            if (fromDate.HasValue)
                payments = payments.Where(p => p.PaymentDate.Date >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                payments = payments.Where(p => p.PaymentDate.Date <= toDate.Value.Date).ToList();

            model.Payments = payments;
            model.TotalIncoming = payments.Where(p => p.IsIncoming).Sum(p => p.Amount);
            model.TotalOutgoing = payments.Where(p => !p.IsIncoming).Sum(p => p.Amount);
            model.TotalCount = payments.Count;

            model.Balances = payments
                .GroupBy(p => new { p.PartyType, p.PartyName })
                .Select(g => new BLL.ViewModels.Payment.PaymentBalanceVM
                {
                    PartyType = g.Key.PartyType,
                    PartyName = g.Key.PartyName,
                    CurrentBalance = g.Last().BalanceAfter,
                    TotalPayments = g.Sum(x => x.Amount),
                    PaymentsCount = g.Count(),
                    TransactionsUrl = g.Select(x => x.RelatedTransactionsUrl).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
                })
                .OrderByDescending(x => x.TotalPayments).ThenBy(x => x.PartyName).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectionsAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CreatePartial", new PaymentFormVM());
            return View(new PaymentFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentFormVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "البيانات غير صحيحة" });
            }

            var result = await _paymentService.CreateAsync(model);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "تم إضافة الدفعة بنجاح" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _paymentService.GetForEditAsync(id);
            if (model == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "لم يتم العثور على الدفعة" });
                return NotFound();
            }

            await PopulateSelectionsAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditPartial", model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentFormVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "البيانات غير صحيحة" });
            }

            var result = await _paymentService.UpdateAsync(model);
            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new { success = true, message = "تم تعديل الدفعة بنجاح" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message });
            }

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var model = await _paymentService.GetDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        private async Task PopulateSelectionsAsync()
        {
            ViewBag.PartyTypes = new List<SelectListItem>
            {
                new("عميل", PaymentPartyTypes.Customer),
                new("مورد", PaymentPartyTypes.Supplier),
                new("موظف", PaymentPartyTypes.Employee),
                new("مصروف", PaymentPartyTypes.Expense)
            };

            ViewBag.Customers = (await _paymentService.GetCustomersAsync()).OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            ViewBag.Suppliers = (await _paymentService.GetSuppliersAsync()).OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
            ViewBag.Employees = (await _paymentService.GetEmployeesAsync()).OrderBy(e => e.Name).Select(e => new SelectListItem(e.Name, e.Id.ToString())).ToList();
            ViewBag.BankAccounts = (await _paymentService.GetBankAccountsAsync()).OrderBy(x => x.AccountName).Select(x => new SelectListItem($"{x.AccountName} - {x.BankName}", x.Id.ToString())).ToList();
            ViewBag.Cashboxes = (await _paymentService.GetCashboxesAsync()).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
            ViewBag.MobileWallets = (await _paymentService.GetMobileWalletsAsync()).OrderBy(x => x.WalletName).Select(x => new SelectListItem($"{x.WalletName} - {x.PhoneNumber}", x.Id.ToString())).ToList();
        }
    }
}
