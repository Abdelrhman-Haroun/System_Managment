using BLL.ViewModels.PaymentMethod;
using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace System_Managment.Controllers
{
    [Authorize]
    public class PaymentMethodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PaymentMethodIndexVM
            {
                BankAccounts = await _context.BankAccounts.Where(x => !x.IsDeleted).OrderBy(x => x.AccountName).ToListAsync(),
                Cashboxes = await _context.CashBoxes.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
                MobileWallets = await _context.MobileWallets.Where(x => !x.IsDeleted).OrderBy(x => x.WalletName).ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string type)
        {
            await Task.CompletedTask;
            return View(new PaymentMethodFormVM { Type = string.IsNullOrWhiteSpace(type) ? "CashBox" : type });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethodFormVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            switch (model.Type)
            {
                case "BankAccount":
                    _context.BankAccounts.Add(new BankAccount { AccountName = model.Name.Trim(), BankName = model.SecondaryName?.Trim() ?? string.Empty, AccountNumber = model.Code?.Trim() ?? string.Empty, Balance = model.Balance, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                    break;
                case "MobileWallet":
                    _context.MobileWallets.Add(new MobileWallet { WalletName = model.Name.Trim(), PhoneNumber = model.Code?.Trim() ?? string.Empty, Balance = model.Balance, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                    break;
                default:
                    _context.CashBoxes.Add(new CashBox { Name = model.Name.Trim(), Balance = model.Balance, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
                    break;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة وسيلة الدفع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string type, int id)
        {
            var model = await LoadFormModelAsync(type, id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentMethodFormVM model)
        {
            if (!ModelState.IsValid) return View(model);

            switch (model.Type)
            {
                case "BankAccount":
                    var account = await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
                    if (account == null) return NotFound();
                    account.AccountName = model.Name.Trim();
                    account.BankName = model.SecondaryName?.Trim() ?? string.Empty;
                    account.AccountNumber = model.Code?.Trim() ?? string.Empty;
                    account.Balance = model.Balance;
                    account.UpdatedAt = DateTime.Now;
                    break;
                case "MobileWallet":
                    var wallet = await _context.MobileWallets.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
                    if (wallet == null) return NotFound();
                    wallet.WalletName = model.Name.Trim();
                    wallet.PhoneNumber = model.Code?.Trim() ?? string.Empty;
                    wallet.Balance = model.Balance;
                    wallet.UpdatedAt = DateTime.Now;
                    break;
                default:
                    var cashbox = await _context.CashBoxes.FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
                    if (cashbox == null) return NotFound();
                    cashbox.Name = model.Name.Trim();
                    cashbox.Balance = model.Balance;
                    cashbox.UpdatedAt = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث وسيلة الدفع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string type, int id)
        {
            switch (type)
            {
                case "BankAccount":
                    var account = await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    if (account != null) { account.IsDeleted = true; account.UpdatedAt = DateTime.Now; }
                    break;
                case "MobileWallet":
                    var wallet = await _context.MobileWallets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    if (wallet != null) { wallet.IsDeleted = true; wallet.UpdatedAt = DateTime.Now; }
                    break;
                default:
                    var cashbox = await _context.CashBoxes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    if (cashbox != null) { cashbox.IsDeleted = true; cashbox.UpdatedAt = DateTime.Now; }
                    break;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف وسيلة الدفع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private async Task<PaymentMethodFormVM?> LoadFormModelAsync(string type, int id)
        {
            switch (type)
            {
                case "BankAccount":
                    var account = await _context.BankAccounts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    return account == null ? null : new PaymentMethodFormVM { Type = type, Id = account.Id, Name = account.AccountName, SecondaryName = account.BankName, Code = account.AccountNumber, Balance = account.Balance };
                case "MobileWallet":
                    var wallet = await _context.MobileWallets.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    return wallet == null ? null : new PaymentMethodFormVM { Type = type, Id = wallet.Id, Name = wallet.WalletName, Code = wallet.PhoneNumber, Balance = wallet.Balance };
                default:
                    var cashbox = await _context.CashBoxes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    return cashbox == null ? null : new PaymentMethodFormVM { Type = "CashBox", Id = cashbox.Id, Name = cashbox.Name, Balance = cashbox.Balance ?? 0 };
            }
        }
    }
}
