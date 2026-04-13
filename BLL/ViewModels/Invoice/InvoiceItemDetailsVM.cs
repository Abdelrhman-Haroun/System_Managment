using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Invoice
{
    public class InvoiceItemDetailsVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductType { get; set; }
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal EffectiveQuantity => ProductType == (int)DAL.Models.ProductType.Count ? Quantity : Weight;
        public decimal TotalPrice => EffectiveQuantity * UnitPrice;
    }
}
