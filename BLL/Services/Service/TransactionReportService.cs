
using BLL.Services.IService;
using BLL.ViewModels.Transactions;
using DAL.Models;
using DAL.Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Service
{
    public class TransactionReportService : ITransactionReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductTransactionVM>> GetProductTransactionsByProductIdAsync(int productId)
        {
            try
            {
                var transactions = await _unitOfWork.ProductTransaction.GetByProductIdAsync(productId);
                var internalUsages = await _unitOfWork.InternalProductUsage.GetByProductIdAsync(productId);
                var product = await _unitOfWork.Product.GetByIdAsync(productId);

                // Map regular transactions
                var transactionList = transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new ProductTransactionVM
                    {
                        Id = t.Id,
                        ProductId = t.ProductId,
                        ProductName = product?.Name ?? "غير معروف",
                        ProductType = t.ProductType ?? product?.ProductType ?? 1,
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        QuantityBefore = t.QuantityBefore,
                        QuantityChanged = t.QuantityChanged,
                        WeightChanged = t.WeightChanged,
                        QuantityAfter = t.QuantityAfter,
                        UnitPrice = t.UnitPrice,
                        TotalAmount = t.TotalAmount,
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        Notes = t.Notes ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .ToList();

                // Map internal usage as transactions
                var internalUsageTransactions = internalUsages
                    .Where(u => !u.IsDeleted)
                    .Select(u => new ProductTransactionVM
                    {
                        Id = u.Id,
                        ProductId = u.ProductId,
                        ProductName = product?.Name ?? "غير معروف",
                        ProductType = u.Product?.ProductType ?? 1,
                        InvoiceId = 0, // No invoice for internal usage
                        TransactionType = "استخدام داخلي",
                        QuantityBefore = u.StockQuantityBefore,
                        QuantityChanged = (u.ProductType == 1) ? u.Quantity : u.Weight, // Negative because it's being removed
                        WeightChanged = u.Weight,
                        QuantityAfter = u.StockQuantityAfter,
                        UnitPrice = u.UnitPrice,
                        TotalAmount = u.TotalCost,
                        ReferenceNumber = u.ReferenceNumber,
                        Notes = $"{u.UsageCategory}",
                        TransactionDate = u.UsageDate,
                        CreatedAt = u.CreatedAt
                    })
                    .ToList();

                // Combine and sort
                var combined = transactionList.Concat(internalUsageTransactions)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                return combined;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product transactions by product ID: {ex.Message}");
                return Enumerable.Empty<ProductTransactionVM>();
            }
        }

        public async Task<IEnumerable<ProductTransactionVM>> GetProductTransactionsByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                var transactions = await _unitOfWork.ProductTransaction.GetByInvoiceIdAsync(invoiceId);

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new ProductTransactionVM
                    {
                        Id = t.Id,
                        ProductId = t.ProductId,
                        ProductName = t.Product?.Name ?? "غير معروف",
                        ProductType = t.ProductType ?? t.Product?.ProductType ?? 1,
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        QuantityBefore = t.QuantityBefore,
                        QuantityChanged = t.QuantityChanged,
                        WeightChanged = t.WeightChanged,
                        QuantityAfter = t.QuantityAfter,
                        UnitPrice = t.UnitPrice,
                        TotalAmount = t.TotalAmount,
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        Notes = t.Notes ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product transactions by invoice ID: {ex.Message}");
                return Enumerable.Empty<ProductTransactionVM>();
            }
        }

        public async Task<IEnumerable<ProductTransactionVM>> GetAllProductTransactionsAsync()
        {
            try
            {
                var transactions = await _unitOfWork.ProductTransaction.GetAllAsync();
                var internalUsages = await _unitOfWork.InternalProductUsage.GetAllAsync();

                // Map regular transactions
                var transactionList = transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new ProductTransactionVM
                    {
                        Id = t.Id,
                        ProductId = t.ProductId,
                        ProductName = t.Product?.Name ?? "غير معروف",
                        ProductType = t.ProductType ?? t.Product?.ProductType ?? 1,
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        QuantityBefore = t.QuantityBefore,
                        QuantityChanged = t.QuantityChanged,
                        WeightChanged = t.WeightChanged,
                        QuantityAfter = t.QuantityAfter,
                        UnitPrice = t.UnitPrice,
                        TotalAmount = t.TotalAmount,
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        Notes = t.Notes ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .ToList();

                // Map internal usage as transactions
                var internalUsageTransactions = internalUsages
                    .Where(u => !u.IsDeleted)
                    .Select(u => new ProductTransactionVM
                    {
                        Id = u.Id,
                        ProductId = u.ProductId,
                        ProductName = u.Product?.Name ?? "غير معروف",
                        ProductType = u.Product?.ProductType ?? 1,
                        InvoiceId = 0,
                        TransactionType = "استخدام داخلي",
                        QuantityBefore = u.StockQuantityBefore,
                        QuantityChanged = (u.ProductType == 1) ? u.Quantity : u.Weight,
                        WeightChanged = u.Weight,
                        QuantityAfter = u.StockQuantityAfter,
                        UnitPrice = u.UnitPrice,
                        TotalAmount = u.TotalCost,
                        ReferenceNumber = u.ReferenceNumber,
                        Notes = $"{u.UsageCategory}",
                        TransactionDate = u.UsageDate,
                        CreatedAt = u.CreatedAt
                    })
                    .ToList();

                // Combine and sort
                var combined = transactionList.Concat(internalUsageTransactions)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                return combined;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all product transactions: {ex.Message}");
                return Enumerable.Empty<ProductTransactionVM>();
            }
        }

        // ========== INTERNAL USAGE TRANSACTIONS VIEW ==========
        /// <summary>
        /// Get all internal usage records shown as transactions
        /// </summary>
        public async Task<IEnumerable<ProductTransactionVM>> GetInternalUsageTransactionsAsync()
        {
            try
            {
                var internalUsages = await _unitOfWork.InternalProductUsage.GetAllAsync();

                return internalUsages
                    .Where(u => !u.IsDeleted)
                    .Select(u => new ProductTransactionVM
                    {
                        Id = u.Id,
                        ProductId = u.ProductId,
                        ProductName = u.Product?.Name ?? "غير معروف",
                        ProductType = (int)(u.Product?.ProductType),
                        InvoiceId = 0,
                        TransactionType = "استخدام داخلي",
                        QuantityBefore = u.StockQuantityBefore,
                        QuantityChanged = (u.ProductType == 1) ? u.Quantity : u.Weight,
                        WeightChanged = u.Weight,
                        QuantityAfter = u.StockQuantityAfter,
                        UnitPrice = u.UnitPrice,
                        TotalAmount = u.TotalCost,
                        ReferenceNumber = u.ReferenceNumber,
                        Notes = $"{u.UsageCategory}",
                        TransactionDate = u.UsageDate,
                        CreatedAt = u.CreatedAt
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting internal usage transactions: {ex.Message}");
                return Enumerable.Empty<ProductTransactionVM>();
            }
        }

        /// <summary>
        /// Get internal usage records for specific product
        /// </summary>
        public async Task<IEnumerable<ProductTransactionVM>> GetInternalUsageByProductAsync(int productId)
        {
            try
            {
                var internalUsages = await _unitOfWork.InternalProductUsage.GetByProductIdAsync(productId);

                return internalUsages
                    .Where(u => !u.IsDeleted)
                    .Select(u => new ProductTransactionVM
                    {
                        Id = u.Id,
                        ProductId = u.ProductId,
                        ProductName = u.Product?.Name ?? "غير معروف",
                        ProductType = (int)(u.Product?.ProductType),
                        InvoiceId = 0,
                        TransactionType = "استخدام داخلي",
                        QuantityBefore = u.StockQuantityBefore,
                        QuantityChanged = (u.ProductType==1)?u.Quantity:u.Weight,
                        WeightChanged = u.Weight,
                        QuantityAfter = u.StockQuantityAfter,
                        UnitPrice = u.UnitPrice,
                        TotalAmount = u.TotalCost,
                        ReferenceNumber = u.ReferenceNumber,
                        Notes = $"{u.UsageCategory}",
                        TransactionDate = u.UsageDate,
                        CreatedAt = u.CreatedAt
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting internal usage by product: {ex.Message}");
                return Enumerable.Empty<ProductTransactionVM>();
            }
        }

        // ========== CUSTOMER TRANSACTIONS ==========
        public async Task<IEnumerable<CustomerTransactionVM>> GetCustomerTransactionsByCustomerIdAsync(int customerId)
        {
            try
            {
                var transactions = await _unitOfWork.CustomerTransaction.GetByCustomerIdAsync(customerId);
                var customer = await _unitOfWork.Customer.GetByIdAsync(customerId);

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new CustomerTransactionVM
                    {
                        Id = t.Id,
                        CustomerId = t.CustomerId,
                        CustomerName = customer?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer transactions by customer ID: {ex.Message}");
                return Enumerable.Empty<CustomerTransactionVM>();
            }
        }

        public async Task<IEnumerable<CustomerTransactionVM>> GetCustomerTransactionsByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                var transactions = await _unitOfWork.CustomerTransaction.GetByInvoiceIdAsync(invoiceId);

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new CustomerTransactionVM
                    {
                        Id = t.Id,
                        CustomerId = t.CustomerId,
                        CustomerName = t.Customer?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer transactions by invoice ID: {ex.Message}");
                return Enumerable.Empty<CustomerTransactionVM>();
            }
        }

        public async Task<IEnumerable<CustomerTransactionVM>> GetAllCustomerTransactionsAsync()
        {
            try
            {
                var transactions = await _unitOfWork.CustomerTransaction.GetAllAsync();

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new CustomerTransactionVM
                    {
                        Id = t.Id,
                        CustomerId = t.CustomerId,
                        CustomerName = t.Customer?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all customer transactions: {ex.Message}");
                return Enumerable.Empty<CustomerTransactionVM>();
            }
        }

        // ========== SUPPLIER TRANSACTIONS ==========
        public async Task<IEnumerable<SupplierTransactionVM>> GetSupplierTransactionsBySupplierIdAsync(int supplierId)
        {
            try
            {
                var transactions = await _unitOfWork.SupplierTransaction.GetBySupplierIdAsync(supplierId);
                var supplier = await _unitOfWork.Supplier.GetByIdAsync(supplierId);

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new SupplierTransactionVM
                    {
                        Id = t.Id,
                        SupplierId = t.SupplierId,
                        SupplierName = supplier?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting supplier transactions by supplier ID: {ex.Message}");
                return Enumerable.Empty<SupplierTransactionVM>();
            }
        }

        public async Task<IEnumerable<SupplierTransactionVM>> GetSupplierTransactionsByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                var transactions = await _unitOfWork.SupplierTransaction.GetByInvoiceIdAsync(invoiceId);

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new SupplierTransactionVM
                    {
                        Id = t.Id,
                        SupplierId = t.SupplierId,
                        SupplierName = t.Supplier?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting supplier transactions by invoice ID: {ex.Message}");
                return Enumerable.Empty<SupplierTransactionVM>();
            }
        }

        public async Task<IEnumerable<SupplierTransactionVM>> GetAllSupplierTransactionsAsync()
        {
            try
            {
                var transactions = await _unitOfWork.SupplierTransaction.GetAllAsync();

                return transactions
                    .Where(t => !t.IsDeleted)
                    .Select(t => new SupplierTransactionVM
                    {
                        Id = t.Id,
                        SupplierId = t.SupplierId,
                        SupplierName = t.Supplier?.Name ?? "غير معروف",
                        InvoiceId = t.InvoiceId,
                        TransactionType = t.TransactionType ?? "",
                        BalanceBefore = t.BalanceBefore,
                        AmountChanged = t.AmountChanged,
                        BalanceAfter = t.BalanceAfter,
                        Description = t.Description ?? "",
                        ReferenceNumber = t.ReferenceNumber ?? "",
                        TransactionDate = t.CreatedAt,
                        CreatedAt = t.CreatedAt
                    })
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all supplier transactions: {ex.Message}");
                return Enumerable.Empty<SupplierTransactionVM>();
            }
        }

        // ========== SUMMARY REPORTS ==========
        public async Task<(decimal TotalIn, decimal TotalOut)> GetProductStockSummaryAsync(int productId)
        {
            try
            {
                var transactions = await _unitOfWork.ProductTransaction.GetByProductIdAsync(productId);
                var internalUsages = await _unitOfWork.InternalProductUsage.GetByProductIdAsync(productId);

                var totalIn = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Purchases")
                    .Sum(t => t.QuantityChanged);

                var totalOut = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Sales")
                    .Sum(t => Math.Abs(t.QuantityChanged));

                // Add internal usage to total out
                var internalOut = internalUsages
                    .Where(u => !u.IsDeleted)
                    .Sum(u => u.Weight);

                return (totalIn, totalOut + internalOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product stock summary: {ex.Message}");
                return (0, 0);
            }
        }

        public async Task<(decimal TotalDebt, int InvoiceCount)> GetCustomerDebtSummaryAsync(int customerId)
        {
            try
            {
                var transactions = await _unitOfWork.CustomerTransaction.GetByCustomerIdAsync(customerId);

                var invoices = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Sales")
                    .GroupBy(t => t.InvoiceId)
                    .Count();

                var totalDebt = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Sales")
                    .Sum(t => t.AmountChanged);

                return (totalDebt, invoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer debt summary: {ex.Message}");
                return (0, 0);
            }
        }

        public async Task<(decimal TotalCredit, int InvoiceCount)> GetSupplierDebtSummaryAsync(int supplierId)
        {
            try
            {
                var transactions = await _unitOfWork.SupplierTransaction.GetBySupplierIdAsync(supplierId);

                var invoices = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Purchases")
                    .GroupBy(t => t.InvoiceId)
                    .Count();

                var totalCredit = transactions
                    .Where(t => !t.IsDeleted && t.TransactionType == "Purchases")
                    .Sum(t => t.AmountChanged);

                return (totalCredit, invoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting supplier credit summary: {ex.Message}");
                return (0, 0);
            }
        }
    }
}