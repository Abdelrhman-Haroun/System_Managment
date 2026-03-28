// ========== BLL/Services/Service/TransactionService.cs (UPDATED TO MATCH YOUR SCHEMA) ==========
using BLL.Services.IService;
using DAL.Models;
using DAL.Repositories.IRepository;

namespace BLL.Services.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogProductTransactionAsync(
            int productId,
            int invoiceId,
            string type,
            decimal quantityBefore,
            decimal quantityChanged,
            decimal weightChanged,
            decimal quantityAfter,
            decimal unitPrice,
            string referenceNumber,
            string notes,
            int? productType = null)
        {
            try
            {
                var transaction = new ProductTransaction
                {
                    ProductId = productId,
                    InvoiceId = invoiceId,
                    TransactionType = type,
                    ProductType = productType,
                    QuantityBefore = quantityBefore,
                    QuantityChanged = quantityChanged,
                    WeightChanged = weightChanged,
                    QuantityAfter = quantityAfter,
                    UnitPrice = unitPrice,
                    TotalAmount = weightChanged * unitPrice,
                    ReferenceNumber = referenceNumber ?? "",
                    Notes = notes ?? "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.ProductTransaction.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Log but don't throw - transaction logging shouldn't break invoice operations
                Console.WriteLine($"خطأ في تسجيل معاملة المنتج: {ex.Message}");
            }
        }

        public async Task LogCustomerTransactionAsync(
            int customerId,
            int invoiceId,
            string type,
            decimal balanceBefore,
            decimal amountChanged,
            decimal balanceAfter,
            string description,
            string referenceNumber)
        {
            try
            {
                var transaction = new CustomerTransaction
                {
                    CustomerId = customerId,
                    InvoiceId = invoiceId,
                    TransactionType = type,
                    BalanceBefore = balanceBefore,
                    AmountChanged = amountChanged,
                    BalanceAfter = balanceAfter,
                    Description = description ?? "",
                    ReferenceNumber = referenceNumber ?? "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.CustomerTransaction.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في تسجيل معاملة العميل: {ex.Message}");
            }
        }

        public async Task LogSupplierTransactionAsync(
            int supplierId,
            int invoiceId,
            string type,
            decimal balanceBefore,
            decimal amountChanged,
            decimal balanceAfter,
            string description,
            string referenceNumber)
        {
            try
            {
                var transaction = new SupplierTransaction
                {
                    SupplierId = supplierId,
                    InvoiceId = invoiceId,
                    TransactionType = type,
                    BalanceBefore = balanceBefore,
                    AmountChanged = amountChanged,
                    BalanceAfter = balanceAfter,
                    Description = description ?? "",
                    ReferenceNumber = referenceNumber ?? "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.SupplierTransaction.AddAsync(transaction);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في تسجيل معاملة المورد: {ex.Message}");
            }
        }

        public async Task<bool> RevertInvoiceTransactionsAsync(int invoiceId)
        {
            try
            {
                // Soft delete all transactions related to this invoice
                var productTransactions = await _unitOfWork.ProductTransaction.GetByInvoiceIdAsync(invoiceId);
                var customerTransactions = await _unitOfWork.CustomerTransaction.GetByInvoiceIdAsync(invoiceId);
                var supplierTransactions = await _unitOfWork.SupplierTransaction.GetByInvoiceIdAsync(invoiceId);

                foreach (var pt in productTransactions)
                {
                    pt.IsDeleted = true;
                    pt.UpdatedAt = DateTime.Now;
                    _unitOfWork.ProductTransaction.Update(pt);
                }

                foreach (var ct in customerTransactions)
                {
                    ct.IsDeleted = true;
                    ct.UpdatedAt = DateTime.Now;
                    _unitOfWork.CustomerTransaction.Update(ct);
                }

                foreach (var st in supplierTransactions)
                {
                    st.IsDeleted = true;
                    st.UpdatedAt = DateTime.Now;
                    _unitOfWork.SupplierTransaction.Update(st);
                }

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في استرجاع معاملات الفاتورة: {ex.Message}");
                return false;
            }
        }
    }
}
