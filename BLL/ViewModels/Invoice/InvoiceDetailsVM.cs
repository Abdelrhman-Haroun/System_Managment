using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class InvoiceDetailsVM
    {
        public int Id { get; set; }
        public string InvoiceType { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }

        // Customer or Supplier info
        public string CustomerOrSupplierName { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        public List<InvoiceItemDetailsVM> Items { get; set; } = new List<InvoiceItemDetailsVM>();

        // Helper properties
        public bool IsPurchase => InvoiceType == "Purchase";
        public bool IsSales => InvoiceType == "Sales";
    }
}
