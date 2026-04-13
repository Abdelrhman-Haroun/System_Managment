using BLL.ViewModels.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IService
{
    public interface ITransactionReportService
    {
        // Product Transactions
        Task<IEnumerable<ProductTransactionVM>> GetProductTransactionsByProductIdAsync(int productId);
        Task<IEnumerable<ProductTransactionVM>> GetProductTransactionsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<ProductTransactionVM>> GetAllProductTransactionsAsync();

        // Customer Transactions
        Task<IEnumerable<CustomerTransactionVM>> GetCustomerTransactionsByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerTransactionVM>> GetCustomerTransactionsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<CustomerTransactionVM>> GetAllCustomerTransactionsAsync();

        // Supplier Transactions
        Task<IEnumerable<SupplierTransactionVM>> GetSupplierTransactionsBySupplierIdAsync(int supplierId);
        Task<IEnumerable<SupplierTransactionVM>> GetSupplierTransactionsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<SupplierTransactionVM>> GetAllSupplierTransactionsAsync();
        
        // Employee Transactions
        Task<IEnumerable<DAL.Models.EmployeeTransaction>> GetEmployeeTransactionsByEmployeeIdAsync(int employeeId);

        // Summary Reports
        Task<(decimal TotalIn, decimal TotalOut)> GetProductStockSummaryAsync(int productId);
        Task<(decimal TotalDebt, int InvoiceCount)> GetCustomerDebtSummaryAsync(int customerId);
        Task<(decimal TotalCredit, int InvoiceCount)> GetSupplierDebtSummaryAsync(int supplierId);

    }
}
