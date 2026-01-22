using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.InternalUsage
{
    public class CreateInternalUsageVM
    {
        [Required(ErrorMessage = "يرجى اختيار المنتج")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الكمية")]
        [Range(0.01, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الوزن")]
        [Range(0.01, double.MaxValue, ErrorMessage = "الوزن يجب أن تكون أكبر من صفر")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "يرجى إدخال سعر الوحدة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الفئة")]
        public string UsageCategory { get; set; }

        [Required(ErrorMessage = "يرجى اختيار تاريخ الاستخدام")]
        public DateTime UsageDate { get; set; }
        public string? Notes { get; set; } // Additional notes
    }
}
