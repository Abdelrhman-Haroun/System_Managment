
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DAL.Repositories.IRepository
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Invoice?> GetInvoiceWithDetailsAsync(int id)
        {
            return await GetFirstOrDefaultAsync(
                predicate: i => i.Id == id && !i.IsDeleted,
                includeWord: "Customer,Supplier,InvoiceItems,InvoiceItems.Product"
            );
        }

        public async Task<IEnumerable<Invoice>> GetAllWithDetailsAsync(string? invoiceType = null)
        {
            if (string.IsNullOrEmpty(invoiceType))
            {
                return await GetAllAsync(
                    predicate: i => !i.IsDeleted,
                    includeWord: "Customer,Supplier,InvoiceItems"
                );
            }
            else
            {
                return await GetAllAsync(
                    predicate: i => !i.IsDeleted && i.InvoiceType == invoiceType,
                    includeWord: "Customer,Supplier,InvoiceItems"
                );
            }
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await GetAllAsync(
                predicate: i => !i.IsDeleted && i.CustomerId == customerId,
                includeWord: "Customer,InvoiceItems,InvoiceItems.Product"
            );
        }

        public async Task<IEnumerable<Invoice>> GetBySupplierIdAsync(int supplierId)
        {
            return await GetAllAsync(
                predicate: i => !i.IsDeleted && i.SupplierId == supplierId,
                includeWord: "Supplier,InvoiceItems,InvoiceItems.Product"
            );
        }

        public async Task<string> GenerateInvoiceNumberAsync(string invoiceType)
        {
            var prefix = invoiceType == InvoiceTypes.Purchase ? "PUR" : "SAL";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            // Get the last invoice number for this type and month
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceType == invoiceType &&
                           i.InvoiceDate.Year == year &&
                           i.InvoiceDate.Month == month)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
            {
                // Extract number from last invoice (e.g., "PUR-2024-01-0005" -> 5)
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"{prefix}-{year}-{month:D2}-{nextNumber:D4}";
        }
    }
}

