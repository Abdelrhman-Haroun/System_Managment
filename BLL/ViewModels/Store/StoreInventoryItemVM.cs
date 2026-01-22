using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Store
{
    public class StoreInventoryItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductType { get; set; }
        public string CategoryName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal AveragePurchasePrice { get; set; }
        public decimal StockValue { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public decimal LastUnitPrice { get; set; }
    }
}
