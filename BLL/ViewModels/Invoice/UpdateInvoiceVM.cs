using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class UpdateInvoiceVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "نوع الفاتورة مطلوب")]
        public string InvoiceType { get; set; }

        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        public DateTime InvoiceDate { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم غير صحيحة")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الضريبة غير صحيحة")]
        public decimal TaxAmount { get; set; } = 0;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "يجب إضافة بند واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل")]
        public List<InvoiceItemVM> Items { get; set; } = new List<InvoiceItemVM>();
    }
}

