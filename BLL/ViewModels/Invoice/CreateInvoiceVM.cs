using DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class CreateInvoiceVM
    {
        [Required(ErrorMessage = "نوع الفاتورة مطلوب")]
        public string InvoiceType { get; set; } // "Purchase" or "Sales"

        // For Sales Invoice
        public int? CustomerId { get; set; }

        // For Purchase Invoice
        public int? SupplierId { get; set; }
        public DateTime InvoiceDate { get; set; }
        // Optional reference number
        [StringLength(100, ErrorMessage = "رقم المرجع يجب أن لا يتجاوز 100 حرف")]
        public string? ReferenceNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم غير صحيحة")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الضريبة غير صحيحة")]
        public decimal TaxAmount { get; set; } = 0;

        [StringLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "يجب إضافة بند واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل")]
        public List<InvoiceItemVM> Items { get; set; } = new List<InvoiceItemVM>();
    }
}
