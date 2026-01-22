using BLL.Services.IService;
using BLL.ViewModels.Invoice;
using DAL.Models;
using DAL.Repositories.IRepository;


namespace BLL.Services.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;

        public InvoiceService(
            IUnitOfWork unitOfWork,
            ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _transactionService = transactionService;
        }

        public async Task<(bool Success, string Message, int? InvoiceId)> CreateInvoiceAsync(CreateInvoiceVM model)
        {
            try
            {
                // Validation
                if (model.InvoiceType == InvoiceTypes.Sales && !model.CustomerId.HasValue)
                {
                    return (false, "يرجى اختيار العميل", null);
                }

                if (model.InvoiceType == InvoiceTypes.Purchase && !model.SupplierId.HasValue)
                {
                    return (false, "يرجى اختيار المورد", null);
                }

                if (model.Items == null || !model.Items.Any())
                {
                    return (false, "يجب إضافة بند واحد على الأقل", null);
                }

                // Calculate totals and create items
                decimal subtotal = 0;
                var invoiceItems = new List<InvoiceItem>();

                foreach (var item in model.Items)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                    if (product == null)
                    {
                        return (false, $"المنتج غير موجود", null);
                    }

                    // Check stock for sales
                    if (model.InvoiceType == InvoiceTypes.Sales)
                    {
                        decimal requiredStock = (product.ProductType == 1) ? item.Quantity : item.Weight;
                        if (product.StockQuantity < requiredStock)
                        {
                            return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.StockQuantity}", null);
                        }
                    }

                    var itemTotal = item.Weight * item.UnitPrice;
                    subtotal += itemTotal;

                    invoiceItems.Add(new InvoiceItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        Weight = item.Weight,
                        UnitPrice = item.UnitPrice
                    });
                }

                decimal totalAmount = subtotal - model.DiscountAmount + model.TaxAmount;

                var invoiceNumber = await _unitOfWork.Invoice.GenerateInvoiceNumberAsync(model.InvoiceType);

                var invoice = new Invoice
                {
                    InvoiceType = model.InvoiceType,
                    InvoiceNumber = invoiceNumber,
                    CustomerId = model.CustomerId,
                    SupplierId = model.SupplierId,
                    InvoiceDate = model.InvoiceDate,
                    ReferenceNumber = model.ReferenceNumber,
                    SubTotal = subtotal,
                    DiscountAmount = model.DiscountAmount,
                    TaxAmount = model.TaxAmount,
                    TotalAmount = totalAmount,
                    Notes = model.Notes,
                    InvoiceItems = invoiceItems,
                };

                _unitOfWork.Invoice.Add(invoice);
                await _unitOfWork.CompleteAsync();

                // Update customer/supplier balance ONCE per invoice
                if (model.InvoiceType == InvoiceTypes.Purchase)
                {
                    var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == invoice.SupplierId);
                    if (supplier != null)
                    {
                        decimal supplierBalanceBefore = (decimal)supplier.Balance;
                        supplier.Balance += invoice.TotalAmount;
                        _unitOfWork.Supplier.Update(supplier);

                        await _transactionService.LogSupplierTransactionAsync(
                            supplier.Id,
                            invoice.Id,
                            "Purchases",
                            supplierBalanceBefore,
                            invoice.TotalAmount,
                            (decimal)supplier.Balance,
                            $"فاتورة مشتريات #{invoiceNumber}",
                            invoiceNumber);
                    }
                }
                else // Sales
                {
                    var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == invoice.CustomerId);
                    if (customer != null)
                    {
                        decimal customerBalanceBefore = (decimal)customer.Balance;
                        customer.Balance += invoice.TotalAmount;
                        _unitOfWork.Customer.Update(customer);

                        await _transactionService.LogCustomerTransactionAsync(
                            customer.Id,
                            invoice.Id,
                            "Sales",
                            customerBalanceBefore,
                            invoice.TotalAmount,
                            (decimal)customer.Balance,
                            $"فاتورة مبيعات #{invoiceNumber}",
                            invoiceNumber);
                    }
                }

                // Process each product item
                foreach (var item in model.Items)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        decimal quantityBefore = (decimal)product.StockQuantity;
                        decimal quantityChanged = 0;
                        decimal weightChanged = item.Weight;

                        // Determine quantity changed based on product type
                        if (product.ProductType == 1)
                            quantityChanged = item.Quantity;
                        else
                            quantityChanged = item.Weight;

                        if (model.InvoiceType == InvoiceTypes.Purchase)
                        {
                            // Purchase: Add to stock
                            if (product.ProductType == 1)
                                product.StockQuantity += item.Quantity;
                            else
                                product.StockQuantity += item.Weight;
                        }
                        else // Sales
                        {
                            // Sales: Subtract from stock
                            if (product.ProductType == 1)
                                product.StockQuantity -= item.Quantity;
                            else
                                product.StockQuantity -= item.Weight;
                        }

                        product.UpdatedAt = DateTime.Now;
                        _unitOfWork.Product.Update(product);

                        // Log product transaction
                        await _transactionService.LogProductTransactionAsync(
                            product.Id,
                            invoice.Id,
                            model.InvoiceType == InvoiceTypes.Purchase ? "Purchases" : "Sales",
                            quantityBefore,
                            quantityChanged,
                            weightChanged,
                            (decimal)product.StockQuantity,
                            item.UnitPrice,
                            invoiceNumber,
                            $"الفاتورة #{invoiceNumber}",
                            product.ProductType);
                    }
                }

                await _unitOfWork.CompleteAsync();

                var typeName = model.InvoiceType == InvoiceTypes.Purchase ? "المشتريات" : "المبيعات";
                return (true, $"تم إنشاء فاتورة {typeName} بنجاح - رقم الفاتورة: {invoiceNumber}", invoice.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateInvoiceAsync(UpdateInvoiceVM model)
        {
            try
            {
                var invoice = await _unitOfWork.Invoice.GetInvoiceWithDetailsAsync(model.Id);
                if (invoice == null)
                {
                    return (false, "الفاتورة غير موجودة");
                }

                // Revert old transactions
                await _transactionService.RevertInvoiceTransactionsAsync(invoice.Id);

                // Revert old balance change
                if (invoice.InvoiceType == InvoiceTypes.Purchase)
                {
                    var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == invoice.SupplierId);
                    if (supplier != null)
                    {
                        supplier.Balance -= invoice.TotalAmount;
                        _unitOfWork.Supplier.Update(supplier);
                    }
                }
                else // Sales
                {
                    var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == invoice.CustomerId);
                    if (customer != null)
                    {
                        customer.Balance -= invoice.TotalAmount;
                        _unitOfWork.Customer.Update(customer);
                    }
                }

                // Revert old stock changes for each item
                foreach (var oldItem in invoice.InvoiceItems.Where(i => !i.IsDeleted))
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == oldItem.ProductId);
                    if (product != null)
                    {
                        if (invoice.InvoiceType == InvoiceTypes.Purchase)
                        {
                            if (product.ProductType == 1)
                                product.StockQuantity -= oldItem.Quantity;
                            else
                                product.StockQuantity -= oldItem.Weight;
                        }
                        else // Sales
                        {
                            if (product.ProductType == 1)
                                product.StockQuantity += oldItem.Quantity;
                            else
                                product.StockQuantity += oldItem.Weight;
                        }

                        product.UpdatedAt = DateTime.Now;
                        _unitOfWork.Product.Update(product);
                    }
                }

                await _unitOfWork.CompleteAsync();

                // Update invoice basic info
                decimal subtotal = model.Items.Sum(i => i.Weight * i.UnitPrice);

                invoice.InvoiceType = model.InvoiceType;
                invoice.CustomerId = model.CustomerId;
                invoice.SupplierId = model.SupplierId;
                invoice.ReferenceNumber = model.ReferenceNumber;
                invoice.SubTotal = subtotal;
                invoice.DiscountAmount = model.DiscountAmount;
                invoice.TaxAmount = model.TaxAmount;
                invoice.TotalAmount = subtotal - model.DiscountAmount + model.TaxAmount;
                invoice.Notes = model.Notes;
                invoice.UpdatedAt = DateTime.Now;

                // Clear old items (soft delete)
                foreach (var oldItem in invoice.InvoiceItems)
                {
                    oldItem.IsDeleted = true;
                    oldItem.UpdatedAt = DateTime.Now;
                }

                // Add new items
                foreach (var item in model.Items)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                    if (product == null) continue;

                    var newItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        Weight = item.Weight,
                        UnitPrice = item.UnitPrice
                    };
                    invoice.InvoiceItems.Add(newItem);
                }

                _unitOfWork.Invoice.Update(invoice);
                await _unitOfWork.CompleteAsync();

                // Apply new supplier/customer balance ONCE
                if (invoice.InvoiceType == InvoiceTypes.Purchase)
                {
                    var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == invoice.SupplierId);
                    if (supplier != null)
                    {
                        decimal supplierBalanceBefore = (decimal)supplier.Balance;
                        supplier.Balance += invoice.TotalAmount;
                        _unitOfWork.Supplier.Update(supplier);

                        await _transactionService.LogSupplierTransactionAsync(
                            supplier.Id,
                            invoice.Id,
                            "Purchases",
                            supplierBalanceBefore,
                            invoice.TotalAmount,
                            (decimal)supplier.Balance,
                            $"تحديث فاتورة مشتريات #{invoice.InvoiceNumber}",
                            invoice.ReferenceNumber ?? invoice.InvoiceNumber);
                    }
                }
                else // Sales
                {
                    var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == invoice.CustomerId);
                    if (customer != null)
                    {
                        decimal customerBalanceBefore = (decimal)customer.Balance;
                        customer.Balance += invoice.TotalAmount;
                        _unitOfWork.Customer.Update(customer);

                        await _transactionService.LogCustomerTransactionAsync(
                            customer.Id,
                            invoice.Id,
                            "Sales",
                            customerBalanceBefore,
                            invoice.TotalAmount,
                            (decimal)customer.Balance,
                            $"تحديث فاتورة مبيعات #{invoice.InvoiceNumber}",
                            invoice.ReferenceNumber ?? invoice.InvoiceNumber);
                    }
                }

                // Apply new stock changes for each item
                foreach (var item in model.Items)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                    if (product == null) continue;

                    decimal quantityBefore = (decimal)product.StockQuantity;
                    decimal quantityChanged = 0;
                    decimal weightChanged = item.Weight;

                    // Determine quantity changed based on product type
                    if (product.ProductType == 1)
                        quantityChanged = item.Quantity;
                    else
                        quantityChanged = item.Weight;

                    if (invoice.InvoiceType == InvoiceTypes.Purchase)
                    {
                        if (product.ProductType == 1)
                            product.StockQuantity += item.Quantity;
                        else
                            product.StockQuantity += item.Weight;
                    }
                    else // Sales
                    {
                        // Check stock availability
                        decimal requiredStock = (product.ProductType == 1) ? item.Quantity : item.Weight;
                        if (product.StockQuantity < requiredStock)
                        {
                            return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.StockQuantity}");
                        }

                        if (product.ProductType == 1)
                            product.StockQuantity -= item.Quantity;
                        else
                            product.StockQuantity -= item.Weight;
                    }

                    product.UpdatedAt = DateTime.Now;
                    _unitOfWork.Product.Update(product);

                    // Log product transaction
                    await _transactionService.LogProductTransactionAsync(
                        product.Id,
                        invoice.Id,
                        invoice.InvoiceType == InvoiceTypes.Purchase ? "Purchases" : "Sales",
                        quantityBefore,
                        quantityChanged,
                        weightChanged,
                        (decimal)product.StockQuantity,
                        item.UnitPrice,
                        invoice.ReferenceNumber ?? invoice.InvoiceNumber,
                        $"تحديث الفاتورة #{invoice.InvoiceNumber}",
                        product.ProductType);
                }

                await _unitOfWork.CompleteAsync();

                var typeName = invoice.InvoiceType == InvoiceTypes.Purchase ? "المشتريات" : "المبيعات";
                return (true, $"تم تحديث فاتورة {typeName} بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteInvoiceAsync(int id)
        {
            try
            {
                var invoice = await _unitOfWork.Invoice.GetInvoiceWithDetailsAsync(id);
                if (invoice == null)
                {
                    return (false, "الفاتورة غير موجودة");
                }

                // Revert all transactions
                await _transactionService.RevertInvoiceTransactionsAsync(invoice.Id);

                // Revert stock changes
                foreach (var item in invoice.InvoiceItems.Where(i => !i.IsDeleted))
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        if (invoice.InvoiceType == InvoiceTypes.Purchase)
                        {
                            if (product.ProductType == 1)
                                product.StockQuantity -= item.Quantity;
                            else
                                product.StockQuantity -= item.Weight;
                        }
                        else // Sales
                        {
                            if (product.ProductType == 1)
                                product.StockQuantity += item.Quantity;
                            else
                                product.StockQuantity += item.Weight;
                        }

                        product.UpdatedAt = DateTime.Now;
                        _unitOfWork.Product.Update(product);
                    }
                }

                // Revert balance changes (MOVED OUTSIDE LOOP - FIX)
                if (invoice.InvoiceType == InvoiceTypes.Purchase)
                {
                    var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == invoice.SupplierId);
                    if (supplier != null)
                    {
                        supplier.Balance -= invoice.TotalAmount;
                        _unitOfWork.Supplier.Update(supplier);
                    }
                }
                else // Sales
                {
                    var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == invoice.CustomerId);
                    if (customer != null)
                    {
                        customer.Balance -= invoice.TotalAmount;
                        _unitOfWork.Customer.Update(customer);
                    }
                }

                // Soft delete invoice and items
                invoice.IsDeleted = true;
                invoice.UpdatedAt = DateTime.Now;
                foreach (var item in invoice.InvoiceItems)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.Now;
                }

                _unitOfWork.Invoice.Update(invoice);
                await _unitOfWork.CompleteAsync();

                var typeName = invoice.InvoiceType == InvoiceTypes.Purchase ? "المشتريات" : "المبيعات";
                return (true, $"تم حذف فاتورة {typeName} بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<InvoiceDetailsVM?> GetInvoiceDetailsAsync(int id)
        {
            var invoice = await _unitOfWork.Invoice.GetInvoiceWithDetailsAsync(id);
            if (invoice == null) return null;

            return new InvoiceDetailsVM
            {
                Id = invoice.Id,
                InvoiceType = invoice.InvoiceType,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                ReferenceNumber = invoice.ReferenceNumber,
                CustomerOrSupplierName = invoice.CustomerOrSupplierName ?? "",
                CustomerId = invoice.CustomerId,
                SupplierId = invoice.SupplierId,
                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                Items = invoice.InvoiceItems.Where(i => !i.IsDeleted).Select(i => new InvoiceItemDetailsVM
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName ?? i.Product?.Name ?? "",
                    Quantity = i.Quantity,
                    Weight = i.Weight,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        public async Task<IEnumerable<InvoiceListVM>> GetAllInvoicesAsync(string? invoiceType = null)
        {
            var invoices = await _unitOfWork.Invoice.GetAllWithDetailsAsync(invoiceType);

            return invoices.Select(i => new InvoiceListVM
            {
                Id = i.Id,
                InvoiceType = i.InvoiceType,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                CustomerOrSupplierName = i.CustomerOrSupplierName ?? "",
                TotalAmount = i.TotalAmount,
                ItemsCount = i.InvoiceItems?.Count(item => !item.IsDeleted) ?? 0,
                CreatedAt = i.CreatedAt
            }).OrderByDescending(i => i.CreatedAt);
        }
    }
}