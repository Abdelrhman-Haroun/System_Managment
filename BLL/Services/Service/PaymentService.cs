using BLL.Services.IService;
using BLL.ViewModels.Payment;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace BLL.Services.Service
{
    public class PaymentService : IPaymentService
    {
        private const string HiddenPaymentInvoiceType = "Payment";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;

        public PaymentService(IUnitOfWork unitOfWork, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _transactionService = transactionService;
        }

        public async Task<IEnumerable<PaymentListVM>> GetAllAsync(string? searchTerm = null)
        {
            var payments = await _unitOfWork.Payment.GetAllWithDetailsAsync();

            var result = payments.Select(payment =>
            {
                var referenceNumber = ExtractReferenceNumber(payment.Notes);
                var isCustomerPayment = payment.CustomerId.HasValue;

                return new PaymentListVM
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    IsIncoming = payment.IsIncoming,
                    PartyName = isCustomerPayment ? payment.Customer?.Name ?? "غير معروف" : payment.Supplier?.Name ?? "غير معروف",
                    PartyType = isCustomerPayment ? "عميل" : "مورد",
                    ReferenceNumber = referenceNumber,
                    Notes = CleanNotes(payment.Notes, referenceNumber),
                    CreatedAt = payment.CreatedAt
                };
            });

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLower();
                result = result.Where(p =>
                    p.PartyName.ToLower().Contains(normalized) ||
                    p.ReferenceNumber.ToLower().Contains(normalized) ||
                    (p.Notes?.ToLower().Contains(normalized) ?? false));
            }

            return result.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<PaymentFormVM?> GetForEditAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdWithDetailsAsync(id);
            if (payment == null)
            {
                return null;
            }

            return new PaymentFormVM
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PartyType = payment.CustomerId.HasValue ? "Customer" : "Supplier",
                CustomerId = payment.CustomerId,
                SupplierId = payment.SupplierId,
                Notes = CleanNotes(payment.Notes, ExtractReferenceNumber(payment.Notes))
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(PaymentFormVM model)
        {
            var validationError = await ValidateModelAsync(model);
            if (validationError != null)
            {
                return (false, validationError);
            }

            var referenceNumber = await GenerateReferenceNumberAsync();
            var hiddenInvoice = new Invoice
            {
                InvoiceType = HiddenPaymentInvoiceType,
                InvoiceNumber = referenceNumber,
                CustomerId = model.PartyType == "Customer" ? model.CustomerId : null,
                SupplierId = model.PartyType == "Supplier" ? model.SupplierId : null,
                InvoiceDate = DateTime.Now,
                ReferenceNumber = referenceNumber,
                SubTotal = model.Amount,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = model.Amount,
                Notes = CombineStoredNotes(referenceNumber, model.Notes),
                IsDeleted = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _unitOfWork.Invoice.Add(hiddenInvoice);
            await _unitOfWork.CompleteAsync();

            var payment = new Payment
            {
                CustomerId = model.PartyType == "Customer" ? model.CustomerId : null,
                SupplierId = model.PartyType == "Supplier" ? model.SupplierId : null,
                Amount = model.Amount,
                IsIncoming = model.PartyType == "Customer",
                PaymentMethod = model.PaymentMethod,
                Notes = CombineStoredNotes(referenceNumber, model.Notes),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _unitOfWork.Payment.Add(payment);
            await ApplyBalanceImpactAsync(model, model.Amount, hiddenInvoice.Id, referenceNumber);
            await _unitOfWork.CompleteAsync();

            return (true, "تم تسجيل الدفعة بنجاح");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PaymentFormVM model)
        {
            var validationError = await ValidateModelAsync(model);
            if (validationError != null)
            {
                return (false, validationError);
            }

            var payment = await _unitOfWork.Payment.GetByIdAsync(model.Id);
            if (payment == null || payment.IsDeleted)
            {
                return (false, "الدفعة غير موجودة");
            }

            var referenceNumber = ExtractReferenceNumber(payment.Notes);
            var hiddenInvoice = await FindHiddenInvoiceAsync(referenceNumber);
            if (hiddenInvoice == null)
            {
                return (false, "تعذر العثور على السجل المحاسبي للدفعة");
            }

            await RevertBalanceImpactAsync(payment, hiddenInvoice.Id);

            hiddenInvoice.CustomerId = model.PartyType == "Customer" ? model.CustomerId : null;
            hiddenInvoice.SupplierId = model.PartyType == "Supplier" ? model.SupplierId : null;
            hiddenInvoice.SubTotal = model.Amount;
            hiddenInvoice.TotalAmount = model.Amount;
            hiddenInvoice.Notes = CombineStoredNotes(referenceNumber, model.Notes);
            hiddenInvoice.UpdatedAt = DateTime.Now;
            _unitOfWork.Invoice.Update(hiddenInvoice);

            payment.CustomerId = model.PartyType == "Customer" ? model.CustomerId : null;
            payment.SupplierId = model.PartyType == "Supplier" ? model.SupplierId : null;
            payment.Amount = model.Amount;
            payment.IsIncoming = model.PartyType == "Customer";
            payment.PaymentMethod = model.PaymentMethod;
            payment.Notes = CombineStoredNotes(referenceNumber, model.Notes);
            payment.UpdatedAt = DateTime.Now;
            _unitOfWork.Payment.Update(payment);

            await ApplyBalanceImpactAsync(model, model.Amount, hiddenInvoice.Id, referenceNumber);
            await _unitOfWork.CompleteAsync();

            return (true, "تم تحديث الدفعة بنجاح");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null || payment.IsDeleted)
            {
                return (false, "الدفعة غير موجودة");
            }

            var referenceNumber = ExtractReferenceNumber(payment.Notes);
            var hiddenInvoice = await FindHiddenInvoiceAsync(referenceNumber);
            if (hiddenInvoice == null)
            {
                return (false, "تعذر العثور على السجل المحاسبي للدفعة");
            }

            await RevertBalanceImpactAsync(payment, hiddenInvoice.Id);

            payment.IsDeleted = true;
            payment.UpdatedAt = DateTime.Now;
            _unitOfWork.Payment.Update(payment);

            hiddenInvoice.UpdatedAt = DateTime.Now;
            _unitOfWork.Invoice.Update(hiddenInvoice);

            await _unitOfWork.CompleteAsync();
            return (true, "تم حذف الدفعة بنجاح");
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _unitOfWork.Customer.GetAllAsync(c => !c.IsDeleted);
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersAsync()
        {
            return await _unitOfWork.Supplier.GetAllAsync(s => !s.IsDeleted);
        }

        private async Task<string?> ValidateModelAsync(PaymentFormVM model)
        {
            if (model.Amount <= 0)
            {
                return "المبلغ يجب أن يكون أكبر من صفر";
            }

            if (model.PartyType == "Customer")
            {
                if (!model.CustomerId.HasValue)
                {
                    return "يرجى اختيار العميل";
                }

                if (await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == model.CustomerId.Value && !c.IsDeleted) == null)
                {
                    return "العميل غير موجود";
                }
            }
            else if (model.PartyType == "Supplier")
            {
                if (!model.SupplierId.HasValue)
                {
                    return "يرجى اختيار المورد";
                }

                if (await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == model.SupplierId.Value && !s.IsDeleted) == null)
                {
                    return "المورد غير موجود";
                }
            }
            else
            {
                return "نوع الجهة غير صحيح";
            }

            return null;
        }

        private async Task<string> GenerateReferenceNumberAsync()
        {
            var monthPrefix = $"PAY-{DateTime.Now:yyyy-MM}-";
            var hiddenPaymentInvoices = await _unitOfWork.Invoice.GetAllAsync(i =>
                i.InvoiceType == HiddenPaymentInvoiceType &&
                i.ReferenceNumber != null &&
                i.ReferenceNumber.StartsWith(monthPrefix));

            var lastNumber = hiddenPaymentInvoices
                .Select(i => i.ReferenceNumber)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r =>
                {
                    var parts = r!.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 0 && int.TryParse(parts[^1], out var number) ? number : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"{monthPrefix}{(lastNumber + 1):D4}";
        }

        private async Task<Invoice?> FindHiddenInvoiceAsync(string referenceNumber)
        {
            return await _unitOfWork.Invoice.GetFirstOrDefaultAsync(i =>
                i.InvoiceType == HiddenPaymentInvoiceType &&
                i.ReferenceNumber == referenceNumber);
        }

        private async Task ApplyBalanceImpactAsync(PaymentFormVM model, decimal amount, int invoiceId, string referenceNumber)
        {
            if (model.PartyType == "Customer" && model.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == model.CustomerId.Value && !c.IsDeleted);
                if (customer != null)
                {
                    var balanceBefore = customer.Balance ?? 0;
                    var balanceAfter = balanceBefore - amount;
                    customer.Balance = balanceAfter;
                    _unitOfWork.Customer.Update(customer);

                    await _transactionService.LogCustomerTransactionAsync(
                        customer.Id,
                        invoiceId,
                        TransactionTypes.Payment,
                        balanceBefore,
                        -amount,
                        balanceAfter,
                        "سداد من العميل",
                        referenceNumber);
                }
            }
            else if (model.PartyType == "Supplier" && model.SupplierId.HasValue)
            {
                var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == model.SupplierId.Value && !s.IsDeleted);
                if (supplier != null)
                {
                    var balanceBefore = supplier.Balance ?? 0;
                    var balanceAfter = balanceBefore - amount;
                    supplier.Balance = balanceAfter;
                    _unitOfWork.Supplier.Update(supplier);

                    await _transactionService.LogSupplierTransactionAsync(
                        supplier.Id,
                        invoiceId,
                        TransactionTypes.Payment,
                        balanceBefore,
                        -amount,
                        balanceAfter,
                        "سداد إلى المورد",
                        referenceNumber);
                }
            }
        }

        private async Task RevertBalanceImpactAsync(Payment payment, int invoiceId)
        {
            if (payment.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == payment.CustomerId.Value && !c.IsDeleted);
                if (customer != null)
                {
                    customer.Balance = (customer.Balance ?? 0) + payment.Amount;
                    _unitOfWork.Customer.Update(customer);
                }
            }
            else if (payment.SupplierId.HasValue)
            {
                var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == payment.SupplierId.Value && !s.IsDeleted);
                if (supplier != null)
                {
                    supplier.Balance = (supplier.Balance ?? 0) + payment.Amount;
                    _unitOfWork.Supplier.Update(supplier);
                }
            }

            await _transactionService.RevertInvoiceTransactionsAsync(invoiceId);
        }

        private static string CombineStoredNotes(string referenceNumber, string? notes)
        {
            var cleanNotes = notes?.Trim();
            return string.IsNullOrWhiteSpace(cleanNotes)
                ? $"[{referenceNumber}]"
                : $"[{referenceNumber}] {cleanNotes}";
        }

        private static string ExtractReferenceNumber(string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
            {
                return string.Empty;
            }

            var start = notes.IndexOf('[');
            var end = notes.IndexOf(']');
            if (start >= 0 && end > start)
            {
                return notes.Substring(start + 1, end - start - 1).Trim();
            }

            return string.Empty;
        }

        private static string? CleanNotes(string? notes, string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(notes))
            {
                return notes;
            }

            var marker = $"[{referenceNumber}]";
            return notes.Replace(marker, string.Empty).Trim();
        }
    }
}
