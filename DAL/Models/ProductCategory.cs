using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class ProductCategory : Base
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
