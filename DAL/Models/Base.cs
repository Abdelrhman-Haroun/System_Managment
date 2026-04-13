using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PaymentMethodType
    {
        Cash,
        BankTransfer,
        Check,
        MobileWallet,
    }

    public static class PaymentMethodTypeExtensions
    {
        public static string GetArabicLabel(this PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.Cash => "نقدي",
                PaymentMethodType.BankTransfer => "تحويل بنكي",
                PaymentMethodType.Check => "شيك",
                PaymentMethodType.MobileWallet => "محفظة إلكترونية",
                _ => paymentMethod.ToString()
            };
        }
    }
}
