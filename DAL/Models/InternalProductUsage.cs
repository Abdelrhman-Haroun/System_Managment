using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class InternalProductUsage : Base
    {
        [Required]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public int? ProductType { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal Quantity { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal Weight { get; set; }    
        [Required]
        [Precision(18, 2)]
        [Column(TypeName = "decimal(18,2)")]  
        public decimal UnitPrice { get; set; }   
        [Required]
        [Precision(18, 2)]
        public decimal TotalCost { get; set; }       
        [Required]
        [MaxLength(100)]
        public string UsageCategory { get; set; } = null!;  
        [Required]
        public DateTime UsageDate { get; set; }
        [StringLength(100)]
        public string ReferenceNumber { get; set; }
        [MaxLength(1000)]  
        public string? Notes { get; set; }      
        [Required]
        [Precision(18, 2)]
        public decimal StockQuantityBefore { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal StockQuantityAfter { get; set; }
    }
}
