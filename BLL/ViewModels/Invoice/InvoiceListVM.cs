using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class InvoiceListVM
    {
        public int Id { get; set; }
        public string InvoiceType { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerOrSupplierName { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }

        // Helper properties
        public bool IsPurchase => InvoiceType == "Purchase";
        public bool IsSales => InvoiceType == "Sales";
        public string TypeDisplayName => IsPurchase ? "مشتريات" : "مبيعات";
    }
}
