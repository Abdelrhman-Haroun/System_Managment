using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.InternalUsage
{
    public class InternalUsageDetailsVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductType { get; set; }
        public decimal Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string UsageCategory { get; set; }
        public DateTime UsageDate { get; set; }
        public string ReferenceNumber { get; set; } // Auto-generated reference number
        public string? Notes { get; set; }
        public decimal StockQuantityBefore { get; set; }
        public decimal StockQuantityAfter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
