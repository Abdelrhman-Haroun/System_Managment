using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class InvoiceItemVM
    {
        [Required(ErrorMessage = "المنتج مطلوب")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "الوزن يجب أن يكون أكبر من صفر")]
        public decimal Weight { get; set; } = 1;

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }
    }
}
