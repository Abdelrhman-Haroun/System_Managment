
//using DAL.Models;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.Identity;

//public class Base
//{
//    [Key]
//    public int Id { get; set; }
//    public bool IsDeleted { get; set; } = false;
//    public DateTime CreatedAt { get; set; } = DateTime.Now;
//}

//public enum PaymentMethodType
//{
//    Cash,
//    BankTransfer,
//    Check,
//    MobileWallet,
//}
//public class ApplicationUser : IdentityUser
//{
//    [Required]
//    [StringLength(50)]
//    public string FullName { get; set; }
//    public bool IsDeleted { get; set; } = false;

//    public DateTime CreatedAt { get; set; } = DateTime.Now;

//    public string? ProfilePicture { get; set; }
//}

//public class BankAccount : Base
//{

//    [StringLength(100)]
//    public string AccountName { get; set; }

//    [StringLength(50)]
//    public string AccountNumber { get; set; }

//    [StringLength(100)]
//    public string BankName { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;
//    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class CashBox : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal? Balance { get; set; } = 0; // Positive = they owe us

//    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class Category : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(500)]
//    public string? Description { get; set; }
//    // Navigation Properties
//    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
//}

//public class Customer : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(20)]
//    public string? Phone { get; set; }

//    [StringLength(255)]
//    public string? Address { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal? Balance { get; set; } = 0; // Positive = they owe us

//    // Navigation Properties
//    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class Employee : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(20)]
//    public string? PhoneNumber { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Salary { get; set; }

//    [StringLength(100)]
//    public string? Position { get; set; }
//}

//public class InvoiceItem : Base
//{
//    public int Quantity { get; set; } = 1;

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal UnitPrice { get; set; }

//    public int? ProductId { get; set; }
//    [ForeignKey(nameof(ProductId))]
//    public Product Product { get; set; }

//    public int? SaleInvoiceId { get; set; }
//    [ForeignKey(nameof(SaleInvoiceId))]
//    public SalesInvoice SaleInvoice { get; set; }

//    public int? PurchaseInvoiceId { get; set; }
//    [ForeignKey(nameof(PurchaseInvoiceId))]
//    public PurchaseInvoice PurchaseInvoice { get; set; }
//}

//public class MobileWallet : Base
//{
//    [StringLength(100)]
//    public string WalletName { get; set; }

//    [StringLength(50)]
//    public string PhoneNumber { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;
//    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class Payment : Base
//{
//    public int? CustomerId { get; set; }
//    [ForeignKey(nameof(CustomerId))]
//    public Customer Customer { get; set; }

//    public int? SupplierId { get; set; }
//    [ForeignKey(nameof(SupplierId))]
//    public Supplier Supplier { get; set; }

//    public int? EmployeeId { get; set; }
//    [ForeignKey(nameof(EmployeeId))]
//    public Employee Employee { get; set; }

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal Amount { get; set; }

//    public bool IsIncoming { get; set; } = true; // true = received, false = paid

//    [Required]
//    public PaymentMethodType PaymentMethod { get; set; }

//    public int? BankAccountId { get; set; }
//    [ForeignKey(nameof(BankAccountId))]
//    public BankAccount BankAccount { get; set; }

//    public int? CashboxId { get; set; }
//    [ForeignKey(nameof(CashboxId))]
//    public CashBox CashBox { get; set; }

//    public int? MobileWalletId { get; set; }
//    [ForeignKey(nameof(MobileWalletId))]
//    public MobileWallet MobileWallet { get; set; }

//    [StringLength(500)]
//    public string Notes { get; set; }
//}

//public class Product : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(500)]
//    public string? Description { get; set; }

//    public int? QuantityInStock { get; set; } = 0;

//    // Store Link
//    [ForeignKey("Store")]
//    public int StoreId { get; set; }
//    [ForeignKey("Caregory")]
//    public int CategoryId { get; set; }

//    // Navigation Properties
//    public virtual Store Store { get; set; }
//    public virtual Category Category { get; set; }
//    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
//}

//public class PurchaseInvoice : Base
//{
//    [ForeignKey("Supplier")]
//    public int SupplierId { get; set; }
//    [ForeignKey("InvoiceItem")]
//    public int InvoiceItemId { get; set; }

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal SubTotal { get; set; } // get from InvoiceItem total of all products

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal DiscountAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TaxAmount { get; set; } = 0;

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal TotalAmount { get; set; }
//    public string? Notes { get; set; }


//    // Navigation Properties
//    public virtual Supplier Supplier { get; set; }
//    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
//}

//public class SalesInvoice : Base
//{
//    [ForeignKey("Customer")]
//    public int CustomerId { get; set; }
//    [ForeignKey("InvoiceItem")]
//    public int InvoiceItemId { get; set; }

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal SubTotal { get; set; } // get from InvoiceItem total of all products

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal DiscountAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TaxAmount { get; set; } = 0;

//    [Required, Column(TypeName = "decimal(18,2)")]
//    public decimal TotalAmount { get; set; }
//    public string? Notes { get; set; }


//    // Navigation Properties
//    public virtual Customer Customer { get; set; }
//    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
//}

//public class Store : Base
//{
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(255)]
//    public string? Description { get; set; }

//    // Navigation Properties
//    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
//}
//public class Supplier : Base
//{
//    [Required(ErrorMessage = "اسم المورد مطلوب")]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(20)]
//    public string? Phone { get; set; }

//    [StringLength(255)]
//    public string? Address { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal? Balance { get; set; } = 0;

//    // Navigation Properties
//    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}





















//// from ai  


//// ==================== BASE CLASSES ====================
//public abstract class BaseEntity
//{
//    [Key]
//    public int Id { get; set; }

//    public bool IsDeleted { get; set; } = false;

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//    public DateTime? UpdatedAt { get; set; }

//    public string? CreatedBy { get; set; }

//    public string? UpdatedBy { get; set; }
//}

//public abstract class AuditableEntity : BaseEntity
//{
//    public string? DeletedBy { get; set; }

//    public DateTime? DeletedAt { get; set; }
//}

//// ==================== ENUMS ====================
//public enum PaymentMethodType
//{
//    Cash = 1,
//    BankTransfer = 2,
//    Check = 3,
//    MobileWallet = 4
//}

//public enum InvoiceStatus
//{
//    Draft = 1,
//    Pending = 2,
//    Paid = 3,
//    PartiallyPaid = 4,
//    Cancelled = 5,
//    Overdue = 6
//}

//public enum PaymentStatus
//{
//    Pending = 1,
//    Completed = 2,
//    Failed = 3,
//    Cancelled = 4
//}

//public enum TransactionType
//{
//    Incoming = 1,  // Receipt
//    Outgoing = 2   // Payment
//}

//// ==================== USER ====================
//public class ApplicationUser : IdentityUser
//{
//    [Required]
//    [StringLength(100)]
//    public string FullName { get; set; }

//    public bool IsDeleted { get; set; } = false;

//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//    [StringLength(500)]
//    public string? ProfilePicture { get; set; }

//    // Navigation Properties
//    public virtual ICollection<SalesInvoice> CreatedSalesInvoices { get; set; } = new List<SalesInvoice>();
//    public virtual ICollection<PurchaseInvoice> CreatedPurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
//}

//// ==================== INVOICE BASE ====================
//public abstract class Invoice : AuditableEntity
//{
//    [Required]
//    [StringLength(50)]
//    public string InvoiceNumber { get; set; }

//    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

//    public DateTime? DueDate { get; set; }

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal SubTotal { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal DiscountAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(5,2)")]
//    public decimal DiscountPercentage { get; set; } = 0;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TaxAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(5,2)")]
//    public decimal TaxPercentage { get; set; } = 0;

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TotalAmount { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal PaidAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal RemainingAmount { get; set; }

//    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

//    [StringLength(1000)]
//    public string? Notes { get; set; }

//    [StringLength(500)]
//    public string? Terms { get; set; }

//    // Navigation Properties
//    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
//    public virtual ICollection<InvoicePayment> InvoicePayments { get; set; } = new List<InvoicePayment>();
//}

//// ==================== SALES INVOICE ====================
//public class SalesInvoice : Invoice
//{
//    [Required]
//    public int CustomerId { get; set; }

//    [ForeignKey(nameof(CustomerId))]
//    public virtual Customer Customer { get; set; }

//    public int? StoreId { get; set; }

//    [ForeignKey(nameof(StoreId))]
//    public virtual Store Store { get; set; }

//    [StringLength(100)]
//    public string? ShippingAddress { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal ShippingCost { get; set; } = 0;
//}

//// ==================== PURCHASE INVOICE ====================
//public class PurchaseInvoice : Invoice
//{
//    [Required]
//    public int SupplierId { get; set; }

//    [ForeignKey(nameof(SupplierId))]
//    public virtual Supplier Supplier { get; set; }

//    [StringLength(50)]
//    public string? SupplierInvoiceNumber { get; set; }

//    public DateTime? ReceivedDate { get; set; }

//    [StringLength(100)]
//    public string? ReceivedBy { get; set; }
//}

//// ==================== INVOICE ITEM ====================
//public class InvoiceItem : AuditableEntity
//{
//    public int? ProductId { get; set; }

//    [ForeignKey(nameof(ProductId))]
//    public virtual Product Product { get; set; }

//    [StringLength(200)]
//    public string ProductName { get; set; }  // Snapshot of product name

//    [StringLength(500)]
//    public string? Description { get; set; }

//    [Required]
//    public int Quantity { get; set; } = 1;

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal UnitPrice { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal DiscountAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(5,2)")]
//    public decimal DiscountPercentage { get; set; } = 0;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TaxAmount { get; set; } = 0;

//    [Column(TypeName = "decimal(5,2)")]
//    public decimal TaxPercentage { get; set; } = 0;

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal TotalPrice { get; set; }  // (Quantity * UnitPrice) - Discount + Tax

//    // Foreign Keys
//    public int? SalesInvoiceId { get; set; }

//    [ForeignKey(nameof(SalesInvoiceId))]
//    public virtual SalesInvoice SalesInvoice { get; set; }

//    public int? PurchaseInvoiceId { get; set; }

//    [ForeignKey(nameof(PurchaseInvoiceId))]
//    public virtual PurchaseInvoice PurchaseInvoice { get; set; }
//}

//// ==================== PAYMENT ====================
//public class Payment : AuditableEntity
//{
//    [Required]
//    [StringLength(50)]
//    public string PaymentNumber { get; set; }

//    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Amount { get; set; }

//    [Required]
//    public TransactionType TransactionType { get; set; }

//    [Required]
//    public PaymentMethodType PaymentMethod { get; set; }

//    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

//    // Party Information (Customer, Supplier, or Employee)
//    public int? CustomerId { get; set; }

//    [ForeignKey(nameof(CustomerId))]
//    public virtual Customer Customer { get; set; }

//    public int? SupplierId { get; set; }

//    [ForeignKey(nameof(SupplierId))]
//    public virtual Supplier Supplier { get; set; }

//    public int? EmployeeId { get; set; }

//    [ForeignKey(nameof(EmployeeId))]
//    public virtual Employee Employee { get; set; }

//    // Payment Method Details
//    public int? BankAccountId { get; set; }

//    [ForeignKey(nameof(BankAccountId))]
//    public virtual BankAccount BankAccount { get; set; }

//    public int? CashBoxId { get; set; }

//    [ForeignKey(nameof(CashBoxId))]
//    public virtual CashBox CashBox { get; set; }

//    public int? MobileWalletId { get; set; }

//    [ForeignKey(nameof(MobileWalletId))]
//    public virtual MobileWallet MobileWallet { get; set; }

//    [StringLength(100)]
//    public string? ReferenceNumber { get; set; }  // Check number, transfer reference, etc.

//    [StringLength(1000)]
//    public string? Notes { get; set; }

//    // Navigation Properties
//    public virtual ICollection<InvoicePayment> InvoicePayments { get; set; } = new List<InvoicePayment>();
//}

//// ==================== INVOICE PAYMENT (JUNCTION TABLE) ====================
//public class InvoicePayment : BaseEntity
//{
//    [Required]
//    public int PaymentId { get; set; }

//    [ForeignKey(nameof(PaymentId))]
//    public virtual Payment Payment { get; set; }

//    public int? SalesInvoiceId { get; set; }

//    [ForeignKey(nameof(SalesInvoiceId))]
//    public virtual SalesInvoice SalesInvoice { get; set; }

//    public int? PurchaseInvoiceId { get; set; }

//    [ForeignKey(nameof(PurchaseInvoiceId))]
//    public virtual PurchaseInvoice PurchaseInvoice { get; set; }

//    [Required]
//    [Column(TypeName = "decimal(18,2)")]
//    public decimal AllocatedAmount { get; set; }  // Amount allocated to this invoice

//    [StringLength(500)]
//    public string? Notes { get; set; }
//}

//// ==================== CUSTOMER ====================
//public class Customer : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(50)]
//    public string? Code { get; set; }  // Customer code/number

//    [StringLength(20)]
//    public string? Phone { get; set; }

//    [StringLength(100)]
//    public string? Email { get; set; }

//    [StringLength(500)]
//    public string? Address { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;  // Positive = they owe us

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal CreditLimit { get; set; } = 0;

//    [StringLength(100)]
//    public string? TaxNumber { get; set; }

//    // Navigation Properties
//    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//// ==================== SUPPLIER ====================
//public class Supplier : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(50)]
//    public string? Code { get; set; }  // Supplier code/number

//    [StringLength(20)]
//    public string? Phone { get; set; }

//    [StringLength(100)]
//    public string? Email { get; set; }

//    [StringLength(500)]
//    public string? Address { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;  // Positive = we owe them

//    [StringLength(100)]
//    public string? TaxNumber { get; set; }

//    [StringLength(100)]
//    public string? ContactPerson { get; set; }

//    // Navigation Properties
//    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//// ==================== PRODUCT ====================
//public class Product : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(50)]
//    public string? SKU { get; set; }  // Stock Keeping Unit

//    [StringLength(50)]
//    public string? Barcode { get; set; }

//    [StringLength(1000)]
//    public string? Description { get; set; }

//    public int QuantityInStock { get; set; } = 0;

//    public int? MinimumStock { get; set; }  // For low stock alerts

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal CostPrice { get; set; }  // Purchase price

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal SellingPrice { get; set; }  // Sales price

//    [StringLength(50)]
//    public string? Unit { get; set; }  // Piece, Kg, Liter, etc.

//    public int StoreId { get; set; }

//    [ForeignKey(nameof(StoreId))]
//    public virtual Store Store { get; set; }

//    public int CategoryId { get; set; }

//    [ForeignKey(nameof(CategoryId))]
//    public virtual Category Category { get; set; }

//    // Navigation Properties
//    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
//}

//// ==================== EMPLOYEE ====================
//public class Employee : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(50)]
//    public string? EmployeeNumber { get; set; }

//    [StringLength(20)]
//    public string? PhoneNumber { get; set; }

//    [StringLength(100)]
//    public string? Email { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Salary { get; set; }

//    [StringLength(100)]
//    public string? Position { get; set; }

//    public DateTime? HireDate { get; set; }

//    // Navigation Properties
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//// ==================== PAYMENT ACCOUNTS ====================
//public class BankAccount : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string AccountName { get; set; }

//    [Required]
//    [StringLength(50)]
//    public string AccountNumber { get; set; }

//    [Required]
//    [StringLength(100)]
//    public string BankName { get; set; }

//    [StringLength(50)]
//    public string? IBAN { get; set; }

//    [StringLength(50)]
//    public string? SwiftCode { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;

//    public bool IsActive { get; set; } = true;

//    // Navigation Properties
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class CashBox : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(500)]
//    public string? Description { get; set; }

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;

//    public bool IsActive { get; set; } = true;

//    // Navigation Properties
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//public class MobileWallet : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string WalletName { get; set; }

//    [Required]
//    [StringLength(50)]
//    public string PhoneNumber { get; set; }

//    [StringLength(50)]
//    public string? Provider { get; set; }  // Vodafone Cash, Orange Money, etc.

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal Balance { get; set; } = 0;

//    public bool IsActive { get; set; } = true;

//    // Navigation Properties
//    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
//}

//// ==================== SUPPORTING ENTITIES ====================
//public class Category : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(1000)]
//    public string? Description { get; set; }

//    public int? ParentCategoryId { get; set; }  // For hierarchical categories

//    [ForeignKey(nameof(ParentCategoryId))]
//    public virtual Category ParentCategory { get; set; }

//    // Navigation Properties
//    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
//    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
//}

//public class Store : AuditableEntity
//{
//    [Required]
//    [StringLength(100)]
//    public string Name { get; set; }

//    [StringLength(500)]
//    public string? Description { get; set; }

//    [StringLength(500)]
//    public string? Address { get; set; }

//    [StringLength(20)]
//    public string? Phone { get; set; }

//    public bool IsActive { get; set; } = true;

//    // Navigation Properties
//    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
//    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
//}