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

        public async Task<PaymentIndexVM> GetAllAsync(string? searchTerm = null)
        {
            var payments = (await _unitOfWork.Payment.GetAllWithDetailsAsync()).ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLowerInvariant();
                payments = payments.Where(payment =>
                        ResolvePartyName(payment).ToLowerInvariant().Contains(normalized) ||
                        ResolveReferenceNumber(payment).ToLowerInvariant().Contains(normalized) ||
                        payment.Reason.ToLowerInvariant().Contains(normalized) ||
                        (payment.Notes?.ToLowerInvariant().Contains(normalized) ?? false) ||
                        GetPartyTypeArabicLabel(GetEffectivePartyType(payment)).ToLowerInvariant().Contains(normalized) ||
                        ResolvePaymentSourceName(payment).ToLowerInvariant().Contains(normalized))
                    .ToList();
            }

            var paymentItems = payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.CreatedAt)
                .Select(payment => new PaymentListVM
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    IsIncoming = payment.IsIncoming,
                    PartyName = ResolvePartyName(payment),
                    PartyType = GetPartyTypeArabicLabel(GetEffectivePartyType(payment)),
                    Reason = string.IsNullOrWhiteSpace(payment.Reason) ? "بدون سبب محدد" : payment.Reason,
                    PaymentSourceName = ResolvePaymentSourceName(payment),
                    ReferenceNumber = ResolveReferenceNumber(payment),
                    BalanceBefore = payment.BalanceBefore,
                    BalanceAfter = payment.BalanceAfter,
                    Notes = payment.Notes,
                    PaymentDate = payment.PaymentDate == default ? payment.CreatedAt : payment.PaymentDate,
                    RelatedTransactionsUrl = BuildTransactionsUrl(payment)
                })
                .ToList();

            return new PaymentIndexVM
            {
                Payments = paymentItems,
                Balances = await BuildBalancesAsync(payments),
                TotalIncoming = payments.Where(p => p.IsIncoming).Sum(p => p.Amount),
                TotalOutgoing = payments.Where(p => !p.IsIncoming).Sum(p => p.Amount),
                TotalCount = payments.Count
            };
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
                PartyType = GetEffectivePartyType(payment),
                CustomerId = payment.CustomerId,
                SupplierId = payment.SupplierId,
                EmployeeId = payment.EmployeeId,
                BankAccountId = payment.BankAccountId,
                CashboxId = payment.CashboxId,
                MobileWalletId = payment.MobileWalletId,
                PartyName = RequiresManualName(GetEffectivePartyType(payment)) ? payment.PartyName : null,
                Reason = string.IsNullOrWhiteSpace(payment.Reason) ? "دفعة" : payment.Reason,
                PaymentDate = payment.PaymentDate == default ? payment.CreatedAt.Date : payment.PaymentDate.Date,
                Notes = payment.Notes
            };
        }

        public async Task<PaymentListVM?> GetDetailsAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdWithDetailsAsync(id);
            if (payment == null) return null;

            return new PaymentListVM
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                IsIncoming = payment.IsIncoming,
                PartyName = ResolvePartyName(payment),
                PartyType = GetPartyTypeArabicLabel(GetEffectivePartyType(payment)),
                Reason = string.IsNullOrWhiteSpace(payment.Reason) ? "بدون سبب محدد" : payment.Reason,
                PaymentSourceName = ResolvePaymentSourceName(payment),
                ReferenceNumber = ResolveReferenceNumber(payment),
                BalanceBefore = payment.BalanceBefore,
                BalanceAfter = payment.BalanceAfter,
                Notes = payment.Notes,
                PaymentDate = payment.PaymentDate == default ? payment.CreatedAt : payment.PaymentDate,
                RelatedTransactionsUrl = BuildTransactionsUrl(payment)
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(PaymentFormVM model)
        {
            NormalizeModel(model);
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
                CustomerId = model.PartyType == PaymentPartyTypes.Customer ? model.CustomerId : null,
                SupplierId = model.PartyType == PaymentPartyTypes.Supplier ? model.SupplierId : null,
                InvoiceDate = model.PaymentDate,
                ReferenceNumber = referenceNumber,
                SubTotal = model.Amount,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = model.Amount,
                Notes = BuildHiddenInvoiceNotes(referenceNumber, model.Reason, model.Notes),
                IsDeleted = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _unitOfWork.Invoice.Add(hiddenInvoice);
            await _unitOfWork.CompleteAsync();

            var payment = new Payment
            {
                PartyType = model.PartyType,
                CustomerId = model.PartyType == PaymentPartyTypes.Customer ? model.CustomerId : null,
                SupplierId = model.PartyType == PaymentPartyTypes.Supplier ? model.SupplierId : null,
                EmployeeId = model.PartyType == PaymentPartyTypes.Employee ? model.EmployeeId : null,
                BankAccountId = model.BankAccountId,
                CashboxId = model.CashboxId,
                MobileWalletId = model.MobileWalletId,
                PartyName = await ResolvePartyNameAsync(model),
                Reason = model.Reason,
                ReferenceNumber = referenceNumber,
                Amount = model.Amount,
                IsIncoming = model.PartyType == PaymentPartyTypes.Customer,
                PaymentMethod = model.PaymentMethod,
                PaymentDate = model.PaymentDate,
                Notes = model.Notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _unitOfWork.Payment.Add(payment);
            await _unitOfWork.CompleteAsync();

            await ApplyBalanceImpactAsync(payment, hiddenInvoice.Id);
            await ApplyPaymentSourceImpactAsync(payment, false);
            await _unitOfWork.CompleteAsync();

            if (IsStandaloneLedgerParty(payment.PartyType))
            {
                await RecalculateStandaloneBalancesAsync(payment);
            }

            return (true, "تم تسجيل الدفعة بنجاح");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PaymentFormVM model)
        {
            NormalizeModel(model);
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

            var oldState = ClonePayment(payment);
            var hiddenInvoice = await FindHiddenInvoiceAsync(ResolveReferenceNumber(payment));
            if (hiddenInvoice == null)
            {
                return (false, "تعذر العثور على السجل المحاسبي للدفعة");
            }

            await RevertBalanceImpactAsync(oldState, hiddenInvoice.Id);
            await ApplyPaymentSourceImpactAsync(oldState, true);

            payment.PartyType = model.PartyType;
            payment.CustomerId = model.PartyType == PaymentPartyTypes.Customer ? model.CustomerId : null;
            payment.SupplierId = model.PartyType == PaymentPartyTypes.Supplier ? model.SupplierId : null;
            payment.EmployeeId = model.PartyType == PaymentPartyTypes.Employee ? model.EmployeeId : null;
            payment.BankAccountId = model.BankAccountId;
            payment.CashboxId = model.CashboxId;
            payment.MobileWalletId = model.MobileWalletId;
            payment.PartyName = await ResolvePartyNameAsync(model);
            payment.Reason = model.Reason;
            payment.Amount = model.Amount;
            payment.IsIncoming = model.PartyType == PaymentPartyTypes.Customer;
            payment.PaymentMethod = model.PaymentMethod;
            payment.PaymentDate = model.PaymentDate;
            payment.Notes = model.Notes;
            payment.UpdatedAt = DateTime.Now;

            hiddenInvoice.CustomerId = payment.CustomerId;
            hiddenInvoice.SupplierId = payment.SupplierId;
            hiddenInvoice.InvoiceDate = model.PaymentDate;
            hiddenInvoice.SubTotal = model.Amount;
            hiddenInvoice.TotalAmount = model.Amount;
            hiddenInvoice.Notes = BuildHiddenInvoiceNotes(payment.ReferenceNumber, model.Reason, model.Notes);
            hiddenInvoice.UpdatedAt = DateTime.Now;

            _unitOfWork.Payment.Update(payment);
            _unitOfWork.Invoice.Update(hiddenInvoice);
            await _unitOfWork.CompleteAsync();

            await ApplyBalanceImpactAsync(payment, hiddenInvoice.Id);
            await ApplyPaymentSourceImpactAsync(payment, false);
            await _unitOfWork.CompleteAsync();

            if (IsStandaloneLedgerParty(oldState.PartyType))
            {
                await RecalculateStandaloneBalancesAsync(oldState);
            }

            if (IsStandaloneLedgerParty(payment.PartyType))
            {
                await RecalculateStandaloneBalancesAsync(payment);
            }

            return (true, "تم تحديث الدفعة بنجاح");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null || payment.IsDeleted)
            {
                return (false, "الدفعة غير موجودة");
            }

            var snapshot = ClonePayment(payment);
            var hiddenInvoice = await FindHiddenInvoiceAsync(ResolveReferenceNumber(payment));
            if (hiddenInvoice == null)
            {
                return (false, "تعذر العثور على السجل المحاسبي للدفعة");
            }

            await RevertBalanceImpactAsync(snapshot, hiddenInvoice.Id);
            await ApplyPaymentSourceImpactAsync(snapshot, true);

            payment.IsDeleted = true;
            payment.UpdatedAt = DateTime.Now;
            _unitOfWork.Payment.Update(payment);
            await _unitOfWork.CompleteAsync();

            if (IsStandaloneLedgerParty(snapshot.PartyType))
            {
                await RecalculateStandaloneBalancesAsync(snapshot);
            }

            return (true, "تم حذف الدفعة بنجاح");
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync() => await _unitOfWork.Customer.GetAllAsync(c => !c.IsDeleted);
        public async Task<IEnumerable<Supplier>> GetSuppliersAsync() => await _unitOfWork.Supplier.GetAllAsync(s => !s.IsDeleted);
        public async Task<IEnumerable<Employee>> GetEmployeesAsync() => await _unitOfWork.Employee.GetAllAsync(e => !e.IsDeleted && e.IsActive, "EmployeeType");
        public async Task<IEnumerable<BankAccount>> GetBankAccountsAsync() => await _unitOfWork.Payment.GetBankAccountsAsync();
        public async Task<IEnumerable<CashBox>> GetCashboxesAsync() => await _unitOfWork.Payment.GetCashboxesAsync();
        public async Task<IEnumerable<MobileWallet>> GetMobileWalletsAsync() => await _unitOfWork.Payment.GetMobileWalletsAsync();

        private async Task<IEnumerable<PaymentBalanceVM>> BuildBalancesAsync(IEnumerable<Payment> payments)
        {
            var result = new List<PaymentBalanceVM>();
            var paymentList = payments.ToList();

            foreach (var group in paymentList.Where(x => GetEffectivePartyType(x) == PaymentPartyTypes.Customer && x.CustomerId.HasValue).GroupBy(x => x.CustomerId!.Value))
            {
                var first = group.First();
                result.Add(new PaymentBalanceVM { PartyType = "عميل", PartyName = first.Customer?.Name ?? first.PartyName, CurrentBalance = first.Customer?.Balance ?? group.Last().BalanceAfter, TotalPayments = group.Sum(x => x.Amount), PaymentsCount = group.Count(), TransactionsUrl = $"/Customer/Transactions/{group.Key}" });
            }

            foreach (var group in paymentList.Where(x => GetEffectivePartyType(x) == PaymentPartyTypes.Supplier && x.SupplierId.HasValue).GroupBy(x => x.SupplierId!.Value))
            {
                var first = group.First();
                result.Add(new PaymentBalanceVM { PartyType = "مورد", PartyName = first.Supplier?.Name ?? first.PartyName, CurrentBalance = first.Supplier?.Balance ?? group.Last().BalanceAfter, TotalPayments = group.Sum(x => x.Amount), PaymentsCount = group.Count(), TransactionsUrl = $"/Supplier/Transactions/{group.Key}" });
            }

            foreach (var group in paymentList.Where(x => GetEffectivePartyType(x) == PaymentPartyTypes.Employee && x.EmployeeId.HasValue).GroupBy(x => x.EmployeeId!.Value))
            {
                var latest = group.Last();
                result.Add(new PaymentBalanceVM {
                    PartyType = "موظف",
                    PartyName = latest.Employee?.Name ?? latest.PartyName,
                    ReasonHint = latest.Reason,
                    CurrentBalance = latest.BalanceAfter,
                    TotalPayments = group.Sum(x => x.Amount),
                    PaymentsCount = group.Count(),
                    TransactionsUrl = $"/Employees/Transactions/{group.Key}"
                });
            }

            foreach (var group in paymentList.Where(x => GetEffectivePartyType(x) == PaymentPartyTypes.Expense).GroupBy(x => x.PartyName))
            {
                var latest = group.Last();
                result.Add(new PaymentBalanceVM { PartyType = "مصروف", PartyName = latest.PartyName, ReasonHint = latest.Reason, CurrentBalance = latest.BalanceAfter, TotalPayments = group.Sum(x => x.Amount), PaymentsCount = group.Count() });
            }

            return result.OrderByDescending(x => x.TotalPayments).ThenBy(x => x.PartyName).ToList();
        }

        private async Task<string?> ValidateModelAsync(PaymentFormVM model)
        {
            if (model.Amount <= 0) return "المبلغ يجب أن يكون أكبر من صفر";
            if (string.IsNullOrWhiteSpace(model.Reason)) return "يرجى كتابة سبب الدفع";
            if (model.PaymentDate == default) return "يرجى اختيار تاريخ صحيح";

            switch (model.PartyType)
            {
                case PaymentPartyTypes.Customer:
                    if (!model.CustomerId.HasValue) return "يرجى اختيار العميل";
                    break;
                case PaymentPartyTypes.Supplier:
                    if (!model.SupplierId.HasValue) return "يرجى اختيار المورد";
                    break;
                case PaymentPartyTypes.Employee:
                    if (!model.EmployeeId.HasValue) return "يرجى اختيار الموظف";
                    break;
                case PaymentPartyTypes.Expense:
                    if (string.IsNullOrWhiteSpace(model.PartyName)) return "يرجى كتابة اسم المصروف أو الجهة";
                    break;
                default:
                    return "نوع الجهة غير صحيح";
            }

            if (model.PaymentMethod == PaymentMethodType.Cash && !model.CashboxId.HasValue) return "يرجى اختيار الخزنة";
            if ((model.PaymentMethod == PaymentMethodType.BankTransfer || model.PaymentMethod == PaymentMethodType.Check) && !model.BankAccountId.HasValue) return "يرجى اختيار الحساب البنكي";
            if (model.PaymentMethod == PaymentMethodType.MobileWallet && !model.MobileWalletId.HasValue) return "يرجى اختيار المحفظة الإلكترونية";

            return null;
        }

        private async Task<string> GenerateReferenceNumberAsync()
        {
            var monthPrefix = $"PAY-{DateTime.Now:yyyy-MM}-";
            var hiddenInvoices = await _unitOfWork.Invoice.GetAllAsync(i => i.InvoiceType == HiddenPaymentInvoiceType && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith(monthPrefix));
            var lastNumber = hiddenInvoices.Select(i => i.ReferenceNumber).Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => { var parts = r!.Split('-', StringSplitOptions.RemoveEmptyEntries); return parts.Length > 0 && int.TryParse(parts[^1], out var number) ? number : 0; }).DefaultIfEmpty(0).Max();
            return $"{monthPrefix}{(lastNumber + 1):D4}";
        }

        private async Task<Invoice?> FindHiddenInvoiceAsync(string referenceNumber) => await _unitOfWork.Invoice.GetFirstOrDefaultAsync(i => i.InvoiceType == HiddenPaymentInvoiceType && i.ReferenceNumber == referenceNumber);

        private async Task ApplyBalanceImpactAsync(Payment payment, int invoiceId)
        {
            if (payment.PartyType == PaymentPartyTypes.Customer && payment.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == payment.CustomerId.Value && !c.IsDeleted);
                if (customer != null)
                {
                    payment.BalanceBefore = customer.Balance ?? 0;
                    payment.BalanceAfter = payment.BalanceBefore - payment.Amount;
                    customer.Balance = payment.BalanceAfter;
                    _unitOfWork.Customer.Update(customer);
                    _unitOfWork.Payment.Update(payment);
                    await _transactionService.LogCustomerTransactionAsync(customer.Id, invoiceId, TransactionTypes.Payment, payment.BalanceBefore, -payment.Amount, payment.BalanceAfter, $"سداد من العميل - {payment.Reason}", payment.ReferenceNumber);
                }
            }
            else if (payment.PartyType == PaymentPartyTypes.Supplier && payment.SupplierId.HasValue)
            {
                var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == payment.SupplierId.Value && !s.IsDeleted);
                if (supplier != null)
                {
                    payment.BalanceBefore = supplier.Balance ?? 0;
                    payment.BalanceAfter = payment.BalanceBefore - payment.Amount;
                    supplier.Balance = payment.BalanceAfter;
                    _unitOfWork.Supplier.Update(supplier);
                    _unitOfWork.Payment.Update(payment);
                    await _transactionService.LogSupplierTransactionAsync(supplier.Id, invoiceId, TransactionTypes.Payment, payment.BalanceBefore, -payment.Amount, payment.BalanceAfter, $"سداد إلى المورد - {payment.Reason}", payment.ReferenceNumber);
                }
            }
            else if (payment.PartyType == PaymentPartyTypes.Employee && payment.EmployeeId.HasValue)
            {
                var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(e => e.Id == payment.EmployeeId.Value && !e.IsDeleted);
                if (employee != null)
                {
                    payment.BalanceBefore = employee.Balance ?? 0;
                    // Treat payment to employee as reducing employee balance (salary paid)
                    payment.BalanceAfter = payment.BalanceBefore - payment.Amount;
                    employee.Balance = payment.BalanceAfter;
                    _unitOfWork.Employee.Update(employee);
                    _unitOfWork.Payment.Update(payment);

                    var empTrans = new EmployeeTransaction
                    {
                        EmployeeId = employee.Id,
                        InvoiceId = invoiceId,
                        TransactionType = TransactionTypes.Payment,
                        BalanceBefore = payment.BalanceBefore,
                        AmountChanged = -payment.Amount,
                        BalanceAfter = payment.BalanceAfter,
                        Description = $"سداد إلى الموظف - {payment.Reason}",
                        ReferenceNumber = payment.ReferenceNumber,
                        TransactionDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _unitOfWork.EmployeeTransaction.Add(empTrans);
                }
            }
            else
            {
                var previous = await _unitOfWork.Payment.GetByPartyAsync(payment.PartyType, payment.CustomerId, payment.SupplierId, payment.EmployeeId, payment.PartyName, payment.Id > 0 ? payment.Id : null);
                payment.BalanceBefore = previous.Sum(x => x.Amount);
                payment.BalanceAfter = payment.BalanceBefore + payment.Amount;
                _unitOfWork.Payment.Update(payment);
            }
        }

        private async Task RevertBalanceImpactAsync(Payment payment, int invoiceId)
        {
            if (payment.PartyType == PaymentPartyTypes.Customer && payment.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == payment.CustomerId.Value && !c.IsDeleted);
                if (customer != null)
                {
                    customer.Balance = (customer.Balance ?? 0) + payment.Amount;
                    _unitOfWork.Customer.Update(customer);
                }
            }
            else if (payment.PartyType == PaymentPartyTypes.Supplier && payment.SupplierId.HasValue)
            {
                var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == payment.SupplierId.Value && !s.IsDeleted);
                if (supplier != null)
                {
                    supplier.Balance = (supplier.Balance ?? 0) + payment.Amount;
                    _unitOfWork.Supplier.Update(supplier);
                }
            }
            else if (payment.PartyType == PaymentPartyTypes.Employee && payment.EmployeeId.HasValue)
            {
                var employee = await _unitOfWork.Employee.GetFirstOrDefaultAsync(e => e.Id == payment.EmployeeId.Value && !e.IsDeleted);
                if (employee != null)
                {
                    // Revert employee balance by adding back the payment amount
                    employee.Balance = (employee.Balance ?? 0) + payment.Amount;
                    _unitOfWork.Employee.Update(employee);

                    // Mark related employee transactions as deleted (revert)
                    var empTransactions = (await _unitOfWork.EmployeeTransaction.GetAllAsync()).Where(t => !t.IsDeleted && (t.InvoiceId == invoiceId || t.ReferenceNumber == payment.ReferenceNumber)).ToList();
                    foreach (var et in empTransactions)
                    {
                        et.IsDeleted = true;
                        et.UpdatedAt = DateTime.Now;
                        _unitOfWork.EmployeeTransaction.Update(et);
                    }
                }
            }

            await _transactionService.RevertInvoiceTransactionsAsync(invoiceId);
        }

        private async Task ApplyPaymentSourceImpactAsync(Payment payment, bool reverse)
        {
            var effect = payment.IsIncoming ? payment.Amount : -payment.Amount;
            if (reverse) effect *= -1;

            var transaction = new PaymentMethodTransaction
            {
                PaymentId = payment.Id,
                AmountChanged = effect,
                ReferenceNumber = payment.ReferenceNumber,
                Description = reverse ? $"إلغاء/تعديل: {payment.Reason}" : payment.Reason,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            if (payment.PaymentMethod == PaymentMethodType.Cash && payment.CashboxId.HasValue)
            {
                var cashbox = (await GetCashboxesAsync()).FirstOrDefault(x => x.Id == payment.CashboxId.Value);
                if (cashbox != null)
                {
                    transaction.SourceType = "Cashbox";
                    transaction.CashboxId = cashbox.Id;
                    transaction.BalanceBefore = cashbox.Balance ?? 0;
                    cashbox.Balance = (cashbox.Balance ?? 0) + effect;
                    transaction.BalanceAfter = cashbox.Balance ?? 0;
                }
            }
            else if ((payment.PaymentMethod == PaymentMethodType.BankTransfer || payment.PaymentMethod == PaymentMethodType.Check) && payment.BankAccountId.HasValue)
            {
                var account = (await GetBankAccountsAsync()).FirstOrDefault(x => x.Id == payment.BankAccountId.Value);
                if (account != null)
                {
                    transaction.SourceType = "BankAccount";
                    transaction.BankAccountId = account.Id;
                    transaction.BalanceBefore = account.Balance;
                    account.Balance += effect;
                    transaction.BalanceAfter = account.Balance;
                }
            }
            else if (payment.PaymentMethod == PaymentMethodType.MobileWallet && payment.MobileWalletId.HasValue)
            {
                var wallet = (await GetMobileWalletsAsync()).FirstOrDefault(x => x.Id == payment.MobileWalletId.Value);
                if (wallet != null)
                {
                    transaction.SourceType = "MobileWallet";
                    transaction.MobileWalletId = wallet.Id;
                    transaction.BalanceBefore = wallet.Balance;
                    wallet.Balance += effect;
                    transaction.BalanceAfter = wallet.Balance;
                }
            }
            
            if (!string.IsNullOrEmpty(transaction.SourceType))
            {
                _unitOfWork.PaymentMethodTransaction.Add(transaction);
            }
        }

        private async Task RecalculateStandaloneBalancesAsync(Payment seedPayment)
        {
            var related = (await _unitOfWork.Payment.GetByPartyAsync(seedPayment.PartyType, seedPayment.CustomerId, seedPayment.SupplierId, seedPayment.EmployeeId, seedPayment.PartyName)).ToList();
            decimal balance = 0;
            foreach (var item in related)
            {
                item.BalanceBefore = balance;
                balance += item.Amount;
                item.BalanceAfter = balance;
                _unitOfWork.Payment.Update(item);
            }
            await _unitOfWork.CompleteAsync();
        }

        private async Task<string> ResolvePartyNameAsync(PaymentFormVM model)
        {
            return model.PartyType switch
            {
                PaymentPartyTypes.Customer => (await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == model.CustomerId))?.Name ?? "عميل",
                PaymentPartyTypes.Supplier => (await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == model.SupplierId))?.Name ?? "مورد",
                PaymentPartyTypes.Employee => (await _unitOfWork.Employee.GetFirstOrDefaultAsync(e => e.Id == model.EmployeeId))?.Name ?? "موظف",
                PaymentPartyTypes.Expense => model.PartyName?.Trim() ?? "مصروف عام",
                _ => model.PartyName?.Trim() ?? "غير معروف"
            };
        }

        private static void NormalizeModel(PaymentFormVM model)
        {
            model.Reason = model.Reason?.Trim() ?? string.Empty;
            model.PartyName = model.PartyName?.Trim();
            model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
            if (model.PaymentMethod != PaymentMethodType.Cash) model.CashboxId = null;
            if (model.PaymentMethod != PaymentMethodType.BankTransfer && model.PaymentMethod != PaymentMethodType.Check) model.BankAccountId = null;
            if (model.PaymentMethod != PaymentMethodType.MobileWallet) model.MobileWalletId = null;
        }

        private static string BuildHiddenInvoiceNotes(string referenceNumber, string reason, string? notes)
        {
            var parts = new List<string> { $"[{referenceNumber}]", $"السبب: {reason}" };
            if (!string.IsNullOrWhiteSpace(notes)) parts.Add(notes.Trim());
            return string.Join(" | ", parts);
        }

        private static string ResolveReferenceNumber(Payment payment) => !string.IsNullOrWhiteSpace(payment.ReferenceNumber) ? payment.ReferenceNumber : string.Empty;
        private static string ResolvePartyName(Payment payment) => !string.IsNullOrWhiteSpace(payment.PartyName) ? payment.PartyName : GetEffectivePartyType(payment) switch { PaymentPartyTypes.Customer => payment.Customer?.Name ?? "عميل غير معروف", PaymentPartyTypes.Supplier => payment.Supplier?.Name ?? "مورد غير معروف", PaymentPartyTypes.Employee => payment.Employee?.Name ?? "موظف غير معروف", PaymentPartyTypes.Expense => "مصروف عام", _ => "جهة غير معروفة" };
        private static string ResolvePaymentSourceName(Payment payment) => payment.PaymentMethod switch { PaymentMethodType.Cash => payment.CashBox?.Name ?? "خزنة", PaymentMethodType.BankTransfer => payment.BankAccount?.AccountName ?? "حساب بنكي", PaymentMethodType.Check => payment.BankAccount?.AccountName ?? "حساب شيكات", PaymentMethodType.MobileWallet => payment.MobileWallet?.WalletName ?? "محفظة إلكترونية", _ => "غير محدد" };
        private static string GetEffectivePartyType(Payment payment) => !string.IsNullOrWhiteSpace(payment.PartyType) ? payment.PartyType : payment.CustomerId.HasValue ? PaymentPartyTypes.Customer : payment.SupplierId.HasValue ? PaymentPartyTypes.Supplier : payment.EmployeeId.HasValue ? PaymentPartyTypes.Employee : PaymentPartyTypes.Expense;
        private static bool RequiresManualName(string? partyType) => partyType == PaymentPartyTypes.Expense;
        private static bool IsStandaloneLedgerParty(string? partyType) => partyType == PaymentPartyTypes.Employee || partyType == PaymentPartyTypes.Expense;
        private static string GetPartyTypeArabicLabel(string? partyType) => partyType switch { PaymentPartyTypes.Customer => "عميل", PaymentPartyTypes.Supplier => "مورد", PaymentPartyTypes.Employee => "موظف", PaymentPartyTypes.Expense => "مصروف", _ => "غير محدد" };
        private static string? BuildTransactionsUrl(Payment payment) => GetEffectivePartyType(payment) switch { PaymentPartyTypes.Customer when payment.CustomerId.HasValue => $"/Customer/Transactions/{payment.CustomerId.Value}", PaymentPartyTypes.Supplier when payment.SupplierId.HasValue => $"/Supplier/Transactions/{payment.SupplierId.Value}", _ => null };
        private static Payment ClonePayment(Payment payment) => new() { Id = payment.Id, PartyType = payment.PartyType, CustomerId = payment.CustomerId, SupplierId = payment.SupplierId, EmployeeId = payment.EmployeeId, BankAccountId = payment.BankAccountId, CashboxId = payment.CashboxId, MobileWalletId = payment.MobileWalletId, PartyName = payment.PartyName, Reason = payment.Reason, ReferenceNumber = payment.ReferenceNumber, Amount = payment.Amount, IsIncoming = payment.IsIncoming, PaymentMethod = payment.PaymentMethod, PaymentDate = payment.PaymentDate, Notes = payment.Notes, BalanceBefore = payment.BalanceBefore, BalanceAfter = payment.BalanceAfter, CreatedAt = payment.CreatedAt, UpdatedAt = payment.UpdatedAt, IsDeleted = payment.IsDeleted };
    }
}
