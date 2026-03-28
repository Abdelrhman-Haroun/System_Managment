using BLL.Services.IService;
using BLL.ViewModels.Invoice;
using DAL.Models;
using DAL.Repositories.IRepository;


namespace BLL.Services.Service
{
    public class InvoiceService : IInvoiceService
    {
        private sealed class PreparedInvoiceItem
        {
            public required Product Product { get; init; }
            public required InvoiceItemVM Item { get; init; }
            public required decimal EffectiveQuantity { get; init; }
        }

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
                var validationMessage = ValidateInvoiceHeader(model.InvoiceType, model.CustomerId, model.SupplierId, model.Items);
                if (validationMessage != null)
                {
                    return (false, validationMessage, null);
                }

                var preparedItems = await PrepareInvoiceItemsAsync(model.Items);
                if (preparedItems == null)
                {
                    return (false, "أحد المنتجات المحددة غير موجود", null);
                }

                var invalidItem = preparedItems.FirstOrDefault(i => i.EffectiveQuantity <= 0 || i.Item.Weight <= 0);
                if (invalidItem != null)
                {
                    return (false, $"بيانات المنتج {invalidItem.Product.Name} غير صحيحة. تأكد من إدخال العدد/الكمية والوزن بشكل صحيح", null);
                }

                if (model.InvoiceType == InvoiceTypes.Sales)
                {
                    foreach (var itemGroup in preparedItems.GroupBy(i => i.Product.Id))
                    {
                        var product = itemGroup.First().Product;
                        var requestedQuantity = itemGroup.Sum(i => i.EffectiveQuantity);
                        var availableQuantity = product.StockQuantity ?? 0;

                        if (availableQuantity < requestedQuantity)
                        {
                            return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {availableQuantity}", null);
                        }
                    }
                }

                var subtotal = preparedItems.Sum(i => i.Item.Weight * i.Item.UnitPrice);
                var invoiceItems = preparedItems.Select(i => new InvoiceItem
                {
                    ProductId = i.Item.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Item.Quantity,
                    Weight = i.Item.Weight,
                    UnitPrice = i.Item.UnitPrice
                }).ToList();

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
                foreach (var preparedItem in preparedItems)
                {
                    var product = preparedItem.Product;
                    decimal quantityBefore = product.StockQuantity ?? 0;
                    decimal quantityChanged = preparedItem.EffectiveQuantity;
                    decimal weightChanged = preparedItem.Item.Weight;

                    product.StockQuantity = model.InvoiceType == InvoiceTypes.Purchase
                        ? quantityBefore + quantityChanged
                        : quantityBefore - quantityChanged;

                    product.UpdatedAt = DateTime.Now;
                    _unitOfWork.Product.Update(product);

                    await _transactionService.LogProductTransactionAsync(
                        product.Id,
                        invoice.Id,
                        model.InvoiceType == InvoiceTypes.Purchase ? "Purchases" : "Sales",
                        quantityBefore,
                        quantityChanged,
                        weightChanged,
                        product.StockQuantity ?? 0,
                        preparedItem.Item.UnitPrice,
                        invoiceNumber,
                        $"الفاتورة #{invoiceNumber}",
                        product.ProductType);
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

                var oldInvoiceType = invoice.InvoiceType;
                var oldSupplierId = invoice.SupplierId;
                var oldCustomerId = invoice.CustomerId;
                var oldTotalAmount = invoice.TotalAmount;

                var validationMessage = ValidateInvoiceHeader(model.InvoiceType, model.CustomerId, model.SupplierId, model.Items);
                if (validationMessage != null)
                {
                    return (false, validationMessage);
                }

                var preparedItems = await PrepareInvoiceItemsAsync(model.Items);
                if (preparedItems == null)
                {
                    return (false, "أحد المنتجات المحددة غير موجود");
                }

                var invalidItem = preparedItems.FirstOrDefault(i => i.EffectiveQuantity <= 0 || i.Item.Weight <= 0);
                if (invalidItem != null)
                {
                    return (false, $"بيانات المنتج {invalidItem.Product.Name} غير صحيحة. تأكد من إدخال العدد/الكمية والوزن بشكل صحيح");
                }

                var oldActiveItems = invoice.InvoiceItems.Where(i => !i.IsDeleted).ToList();
                var stockTargets = new Dictionary<int, decimal>();
                var productCache = new Dictionary<int, Product>();

                foreach (var oldItem in oldActiveItems)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == oldItem.ProductId && !p.IsDeleted);
                    if (product == null)
                    {
                        return (false, "أحد منتجات الفاتورة القديمة غير موجود");
                    }

                    productCache[product.Id] = product;

                    if (!stockTargets.ContainsKey(product.Id))
                    {
                        stockTargets[product.Id] = product.StockQuantity ?? 0;
                    }

                    stockTargets[product.Id] -= GetStockImpact(invoice.InvoiceType, GetEffectiveQuantity(product.ProductType, oldItem.Quantity, oldItem.Weight));
                }

                foreach (var itemGroup in preparedItems.GroupBy(i => i.Product.Id))
                {
                    var product = itemGroup.First().Product;
                    productCache[product.Id] = product;

                    if (!stockTargets.ContainsKey(product.Id))
                    {
                        stockTargets[product.Id] = product.StockQuantity ?? 0;
                    }

                    stockTargets[product.Id] += itemGroup.Sum(i => GetStockImpact(model.InvoiceType, i.EffectiveQuantity));
                }

                var invalidStock = stockTargets.FirstOrDefault(kvp => kvp.Value < 0);
                if (!invalidStock.Equals(default(KeyValuePair<int, decimal>)))
                {
                    var productName = productCache.TryGetValue(invalidStock.Key, out var product)
                        ? product.Name
                        : "المنتج";
                    return (false, $"الكمية المتوفرة من {productName} غير كافية للتعديل");
                }

                var subtotal = preparedItems.Sum(i => i.Item.Weight * i.Item.UnitPrice);
                var totalAmount = subtotal - model.DiscountAmount + model.TaxAmount;

                await _transactionService.RevertInvoiceTransactionsAsync(invoice.Id);

                invoice.InvoiceType = model.InvoiceType;
                invoice.CustomerId = model.CustomerId;
                invoice.SupplierId = model.SupplierId;
                invoice.InvoiceDate = model.InvoiceDate;
                invoice.ReferenceNumber = model.ReferenceNumber;
                invoice.SubTotal = subtotal;
                invoice.DiscountAmount = model.DiscountAmount;
                invoice.TaxAmount = model.TaxAmount;
                invoice.TotalAmount = totalAmount;
                invoice.Notes = model.Notes;
                invoice.UpdatedAt = DateTime.Now;

                foreach (var oldItem in invoice.InvoiceItems)
                {
                    oldItem.IsDeleted = true;
                    oldItem.UpdatedAt = DateTime.Now;
                }

                foreach (var preparedItem in preparedItems)
                {
                    var newItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = preparedItem.Item.ProductId,
                        ProductName = preparedItem.Product.Name,
                        Quantity = preparedItem.Item.Quantity,
                        Weight = preparedItem.Item.Weight,
                        UnitPrice = preparedItem.Item.UnitPrice
                    };
                    invoice.InvoiceItems.Add(newItem);
                }

                await AdjustInvoicePartyBalanceAsync(oldInvoiceType, oldSupplierId, oldCustomerId, -oldTotalAmount);
                await AdjustInvoicePartyBalanceAsync(model.InvoiceType, model.SupplierId, model.CustomerId, totalAmount);

                foreach (var stockTarget in stockTargets)
                {
                    var product = productCache[stockTarget.Key];
                    product.StockQuantity = stockTarget.Value;
                    product.UpdatedAt = DateTime.Now;
                    _unitOfWork.Product.Update(product);
                }

                _unitOfWork.Invoice.Update(invoice);
                await _unitOfWork.CompleteAsync();

                var referenceNumber = invoice.ReferenceNumber ?? invoice.InvoiceNumber ?? string.Empty;

                if (invoice.InvoiceType == InvoiceTypes.Purchase)
                {
                    var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == invoice.SupplierId);
                    if (supplier != null)
                    {
                        var balanceBefore = (supplier.Balance ?? 0) - invoice.TotalAmount;
                        await _transactionService.LogSupplierTransactionAsync(
                            supplier.Id,
                            invoice.Id,
                            "Purchases",
                            balanceBefore,
                            invoice.TotalAmount,
                            supplier.Balance ?? 0,
                            $"تحديث فاتورة مشتريات #{invoice.InvoiceNumber}",
                            referenceNumber);
                    }
                }
                else
                {
                    var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == invoice.CustomerId);
                    if (customer != null)
                    {
                        var balanceBefore = (customer.Balance ?? 0) - invoice.TotalAmount;
                        await _transactionService.LogCustomerTransactionAsync(
                            customer.Id,
                            invoice.Id,
                            "Sales",
                            balanceBefore,
                            invoice.TotalAmount,
                            customer.Balance ?? 0,
                            $"تحديث فاتورة مبيعات #{invoice.InvoiceNumber}",
                            referenceNumber);
                    }
                }

                foreach (var itemGroup in preparedItems.GroupBy(i => i.Product.Id))
                {
                    var product = productCache[itemGroup.Key];
                    var quantityChanged = itemGroup.Sum(i => i.EffectiveQuantity);
                    var weightChanged = itemGroup.Sum(i => i.Item.Weight);
                    var quantityAfter = product.StockQuantity ?? 0;
                    var quantityBefore = quantityAfter - GetStockImpact(invoice.InvoiceType, quantityChanged);
                    var unitPrice = itemGroup.Last().Item.UnitPrice;

                    await _transactionService.LogProductTransactionAsync(
                        product.Id,
                        invoice.Id,
                        invoice.InvoiceType == InvoiceTypes.Purchase ? "Purchases" : "Sales",
                        quantityBefore,
                        quantityChanged,
                        weightChanged,
                        quantityAfter,
                        unitPrice,
                        referenceNumber,
                        $"تحديث الفاتورة #{invoice.InvoiceNumber}",
                        product.ProductType);
                }

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

                var activeItems = invoice.InvoiceItems.Where(i => !i.IsDeleted).ToList();
                foreach (var item in activeItems)
                {
                    var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                    if (product == null)
                    {
                        return (false, "أحد منتجات الفاتورة غير موجود");
                    }

                    var effectiveQuantity = GetEffectiveQuantity(product.ProductType, item.Quantity, item.Weight);
                    var finalQuantity = (product.StockQuantity ?? 0) - GetStockImpact(invoice.InvoiceType, effectiveQuantity);
                    if (finalQuantity < 0)
                    {
                        return (false, $"لا يمكن حذف الفاتورة لأن مخزون المنتج {product.Name} لم يعد يسمح بالتراجع عن هذه العملية");
                    }

                    product.StockQuantity = finalQuantity;
                    product.UpdatedAt = DateTime.Now;
                    _unitOfWork.Product.Update(product);
                }

                await _transactionService.RevertInvoiceTransactionsAsync(invoice.Id);
                await AdjustInvoicePartyBalanceAsync(invoice.InvoiceType, invoice.SupplierId, invoice.CustomerId, -invoice.TotalAmount);

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
                    ProductType = i.Product?.ProductType ?? (int)ProductType.Count,
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

        private static decimal GetEffectiveQuantity(int productType, decimal quantity, decimal weight)
        {
            return productType == (int)ProductType.Count ? quantity : weight;
        }

        private static decimal GetStockImpact(string invoiceType, decimal effectiveQuantity)
        {
            return invoiceType == InvoiceTypes.Purchase ? effectiveQuantity : -effectiveQuantity;
        }

        private static string? ValidateInvoiceHeader(string invoiceType, int? customerId, int? supplierId, List<InvoiceItemVM>? items)
        {
            if (invoiceType == InvoiceTypes.Sales && !customerId.HasValue)
            {
                return "يرجى اختيار العميل";
            }

            if (invoiceType == InvoiceTypes.Purchase && !supplierId.HasValue)
            {
                return "يرجى اختيار المورد";
            }

            if (items == null || !items.Any())
            {
                return "يجب إضافة بند واحد على الأقل";
            }

            return null;
        }

        private async Task<List<PreparedInvoiceItem>?> PrepareInvoiceItemsAsync(IEnumerable<InvoiceItemVM> items)
        {
            var preparedItems = new List<PreparedInvoiceItem>();

            foreach (var item in items)
            {
                var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                if (product == null)
                {
                    return null;
                }

                preparedItems.Add(new PreparedInvoiceItem
                {
                    Product = product,
                    Item = item,
                    EffectiveQuantity = GetEffectiveQuantity(product.ProductType, item.Quantity, item.Weight)
                });
            }

            return preparedItems;
        }

        private async Task AdjustInvoicePartyBalanceAsync(string invoiceType, int? supplierId, int? customerId, decimal delta)
        {
            if (invoiceType == InvoiceTypes.Purchase && supplierId.HasValue)
            {
                var supplier = await _unitOfWork.Supplier.GetFirstOrDefaultAsync(s => s.Id == supplierId.Value);
                if (supplier != null)
                {
                    supplier.Balance = (supplier.Balance ?? 0) + delta;
                    _unitOfWork.Supplier.Update(supplier);
                }
            }
            else if (invoiceType == InvoiceTypes.Sales && customerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetFirstOrDefaultAsync(c => c.Id == customerId.Value);
                if (customer != null)
                {
                    customer.Balance = (customer.Balance ?? 0) + delta;
                    _unitOfWork.Customer.Update(customer);
                }
            }
        }
    }
}
