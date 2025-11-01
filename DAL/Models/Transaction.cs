namespace DAL.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string EntityType { get; set; } // "Customer" or "Supplier"
        public int EntityId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsDebit { get; set; } // true for debit, false for credit
    }
}
