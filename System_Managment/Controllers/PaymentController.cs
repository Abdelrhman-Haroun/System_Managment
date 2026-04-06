using BLL.Services.IService;
using BLL.ViewModels.Payment;
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

        public async Task<IActionResult> Index(string? searchTerm)
        {
            ViewBag.SearchTerm = searchTerm;
            return View(await _paymentService.GetAllAsync(searchTerm));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSelectionsAsync();
            return View(new PaymentFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentFormVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync();
                return View(model);
            }

            var result = await _paymentService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await PopulateSelectionsAsync();
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _paymentService.GetForEditAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            await PopulateSelectionsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentFormVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync();
                return View(model);
            }

            var result = await _paymentService.UpdateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await PopulateSelectionsAsync();
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSelectionsAsync()
        {
            ViewBag.Customers = (await _paymentService.GetCustomersAsync())
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();

            ViewBag.Suppliers = (await _paymentService.GetSuppliersAsync())
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
                .ToList();
        }
    }
}
