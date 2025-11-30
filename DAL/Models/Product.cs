using DAL.Models;
using System.ComponentModel.DataAnnotations;

public class Product : Base
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    // نحدد نوع المنتج (وزن أو عدد)
    public int ProductType { get; set; }

    // للمخزون — حسب النوع
    public decimal? StockQuantity { get; set; } = 0;

    public int StoreId { get; set; }
    public Store Store { get; set; }

    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; }

    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
public enum ProductType
{
    [Display(Name = "العدد")]
    Count = 1,

    [Display(Name = "الوزن (kg)")]
    Weight = 2
}