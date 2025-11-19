using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Product : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? QuantityInStock { get; set; } = 0;

        // Store Link
        [ForeignKey("Store")]
        public int StoreId { get; set; }
        [ForeignKey("Caregory")]
        public int CategoryId { get; set; }

        // Navigation Properties
        public virtual Store Store { get; set; } 
        public virtual Category Category { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
