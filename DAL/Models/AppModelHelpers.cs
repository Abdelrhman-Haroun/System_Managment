using System.Globalization;

namespace DAL.Models;

public static class TransactionTypes
{
    public const string Purchase = InvoiceTypes.Purchase;
    public const string Sales = InvoiceTypes.Sales;
    public const string Payment = "Payment";
    public const string InternalUsage = "InternalUsage";

    public static string Normalize(string? transactionType)
    {
        if (string.IsNullOrWhiteSpace(transactionType))
            return string.Empty;

        var normalized = transactionType.Trim().ToLowerInvariant();

        return normalized switch
        {
            "purchase" or "purchases" => Purchase,
            "sale" or "sales" or "invoice" or "invoices" => Sales,
            "payment" or "payments" => Payment,
            "internalusage" or "internal usage" or "استخدام داخلي" => InternalUsage,
            _ => transactionType.Trim()
        };
    }

    public static bool IsPurchase(string? transactionType) => Normalize(transactionType) == Purchase;
    public static bool IsSales(string? transactionType) => Normalize(transactionType) == Sales;
    public static bool IsPayment(string? transactionType) => Normalize(transactionType) == Payment;
    public static bool IsInternalUsage(string? transactionType) => Normalize(transactionType) == InternalUsage;

    public static string GetArabicLabel(string? transactionType)
    {
        return Normalize(transactionType) switch
        {
            Purchase => "شراء",
            Sales => "بيع",
            Payment => "دفعة",
            InternalUsage => "استخدام داخلي",
            _ => transactionType?.Trim() ?? string.Empty
        };
    }
}

public static class ProductTypeExtensions
{
    public static bool IsCountType(int? productType) => productType == (int)ProductType.Count;

    public static bool IsWeightType(int? productType) => productType == (int)ProductType.Weight;

    public static decimal GetLowStockThreshold(int? productType) => IsCountType(productType) ? 10m : 1000m;

    public static bool IsLowStock(int? productType, decimal currentStock) => currentStock < GetLowStockThreshold(productType);

    public static string FormatStock(int? productType, decimal quantity)
    {
        return IsCountType(productType)
            ? quantity.ToString("#,##0", CultureInfo.InvariantCulture)
            : $"{quantity.ToString("#,##0.##", CultureInfo.InvariantCulture)} كجم";
    }

    public static string GetArabicLabel(int? productType) => IsCountType(productType) ? "عددي" : "وزني";

    public static string GetUsageArabicLabel(int? productType) => IsCountType(productType) ? "وحدات" : "وزن";
}
