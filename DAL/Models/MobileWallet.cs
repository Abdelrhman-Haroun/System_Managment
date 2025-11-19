using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class MobileWallet : Base
    {
        [StringLength(100)]
        public string WalletName { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
