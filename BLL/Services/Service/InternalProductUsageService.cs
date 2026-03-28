using BLL.Services.IService;
using BLL.ViewModels.InternalUsage;
using DAL.Models;
using DAL.Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Service
{
    public class InternalProductUsageService : IInternalProductUsageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;

        public InternalProductUsageService(
            IUnitOfWork unitOfWork,
            ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _transactionService = transactionService;
        }

        public async Task<(bool Success, string Message, int? UsageId)> RecordInternalUsageAsync(CreateInternalUsageVM model)
        {
            try
            {
                if (model.ProductId <= 0)
                    return (false, "يرجى اختيار منتج صحيح", null);

                var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(
                    p => p.Id == model.ProductId && !p.IsDeleted);

                if (product == null)
                    return (false, "المنتج غير موجود", null);

                decimal requiredStock = GetEffectiveQuantity(product.ProductType, model.Quantity, model.Weight);
                if (requiredStock <= 0)
                    return (false, "الكمية المستخدمة يجب أن تكون أكبر من صفر", null);

                if ((product.StockQuantity ?? 0) < requiredStock)
                {
                    return (false,
                        $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.StockQuantity:N2}",
                        null);
                }

                decimal stockBefore = product.StockQuantity ?? 0;
                var referenceNumber = await _unitOfWork.InternalProductUsage.GenerateReferenceNumberAsync();

                var usage = new InternalProductUsage
                {
                    ProductId = model.ProductId,
                    ProductType = product.ProductType,
                    Quantity = model.Quantity,
                    Weight = model.Weight,
                    UnitPrice = model.UnitPrice,
                    TotalCost = requiredStock * model.UnitPrice,
                    UsageCategory = model.UsageCategory ?? "عام",
                    UsageDate = model.UsageDate,
                    ReferenceNumber = referenceNumber,
                    Notes = model.Notes,
                    StockQuantityBefore = stockBefore,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                decimal quantityChanged = requiredStock;

                product.StockQuantity = stockBefore - requiredStock;

                usage.StockQuantityAfter = product.StockQuantity ?? 0;
                product.UpdatedAt = DateTime.Now;

                _unitOfWork.InternalProductUsage.Add(usage);
                _unitOfWork.Product.Update(product);
                await _unitOfWork.CompleteAsync();

                // Log as Product Transaction
                await _transactionService.LogProductTransactionAsync(
                    productId: product.Id,
                    invoiceId: 0, // No invoice for internal usage
                    type: "Internal Usage",
                    quantityBefore: stockBefore,
                    quantityChanged: quantityChanged,
                    weightChanged: model.Weight,
                    quantityAfter: product.StockQuantity ?? 0,
                    unitPrice: model.UnitPrice,
                    referenceNumber: referenceNumber,
                    notes: $"{model.UsageCategory} - {referenceNumber}",
                    productType: product.ProductType
                );

                return (true, $"تم تسجيل الاستخدام الداخلي بنجاح - {product.Name}", usage.Id);
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateInternalUsageAsync(UpdateInternalUsageVM model)
        {
            try
            {
                if (model.ProductId <= 0)
                    return (false, "يرجى اختيار منتج صحيح");

                var usage = await _unitOfWork.InternalProductUsage.GetByIdAsync(model.Id);
                if (usage == null || usage.IsDeleted)
                    return (false, "السجل غير موجود");

                var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(
                    p => p.Id == model.ProductId && !p.IsDeleted);

                if (product == null)
                    return (false, "المنتج غير موجود");

                // Restore old stock first
                var restoredQuantity = GetEffectiveQuantity(product.ProductType, usage.Quantity, usage.Weight);
                product.StockQuantity = (product.StockQuantity ?? 0) + restoredQuantity;

                decimal requiredStock = GetEffectiveQuantity(product.ProductType, model.Quantity, model.Weight);
                if (requiredStock <= 0)
                    return (false, "الكمية المستخدمة يجب أن تكون أكبر من صفر");

                if ((product.StockQuantity ?? 0) < requiredStock)
                {
                    return (false, $"الكمية المتوفرة من {product.Name} غير كافية. المتوفر: {product.StockQuantity:N2}");
                }

                decimal stockBefore = product.StockQuantity ?? 0;

                product.StockQuantity = stockBefore - requiredStock;

                // Update usage record
                usage.ProductId = model.ProductId;
                usage.ProductType = product.ProductType;
                usage.Quantity = model.Quantity;
                usage.Weight = model.Weight;
                usage.UnitPrice = model.UnitPrice;
                usage.TotalCost = requiredStock * model.UnitPrice;
                usage.UsageCategory = model.UsageCategory ?? "عام";
                usage.UsageDate = model.UsageDate;
                usage.Notes = model.Notes;
                usage.StockQuantityBefore = stockBefore;
                usage.StockQuantityAfter = product.StockQuantity ?? 0;
                usage.UpdatedAt = DateTime.Now;

                product.UpdatedAt = DateTime.Now;

                _unitOfWork.InternalProductUsage.Update(usage);
                _unitOfWork.Product.Update(product);
                await _unitOfWork.CompleteAsync();

                decimal quantityChanged = requiredStock;

                // Log updated transaction
                await _transactionService.LogProductTransactionAsync(
                    productId: product.Id,
                    invoiceId: 0, // No invoice for internal usage
                    type: "Internal Usage - Update",
                    quantityBefore: stockBefore,
                    quantityChanged: quantityChanged,
                    weightChanged: model.Weight,
                    quantityAfter: product.StockQuantity ?? 0,
                    unitPrice: model.UnitPrice,
                    referenceNumber: usage.ReferenceNumber,
                    notes: $"تحديث: {model.UsageCategory} - {usage.ReferenceNumber}",
                    productType: product.ProductType
                );

                return (true, $"تم تحديث السجل بنجاح - {product.Name}");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteInternalUsageAsync(int id)
        {
            try
            {
                var usage = await _unitOfWork.InternalProductUsage.GetByIdAsync(id);
                if (usage == null || usage.IsDeleted)
                    return (false, "السجل غير موجود");

                var product = await _unitOfWork.Product.GetByIdAsync(usage.ProductId);
                if (product == null)
                    return (false, "المنتج غير موجود");

                decimal stockBefore = product.StockQuantity ?? 0;

                // Restore stock
                var quantityRestored = GetEffectiveQuantity(product.ProductType, usage.Quantity, usage.Weight);
                product.StockQuantity = stockBefore + quantityRestored;

                product.UpdatedAt = DateTime.Now;

                // Soft delete
                usage.IsDeleted = true;
                usage.UpdatedAt = DateTime.Now;

                _unitOfWork.InternalProductUsage.Update(usage);
                _unitOfWork.Product.Update(product);
                await _unitOfWork.CompleteAsync();

                // Log deletion transaction
                await _transactionService.LogProductTransactionAsync(
                    productId: product.Id,
                    invoiceId: 0,
                    type: "Internal Usage - Delete",
                    quantityBefore: stockBefore,
                    quantityChanged: quantityRestored,
                    weightChanged: usage.Weight,
                    quantityAfter: product.StockQuantity ?? 0,
                    unitPrice: usage.UnitPrice,
                    referenceNumber: usage.ReferenceNumber,
                    notes: $"حذف استخدام: {usage.UsageCategory} - {usage.ReferenceNumber}",
                    productType: product.ProductType
                );

                return (true, "تم حذف السجل واستعادة المخزون بنجاح");
            }
            catch (Exception ex)
            {
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        public async Task<InternalUsageDetailsVM> GetUsageDetailsAsync(int id)
        {
            try
            {
                // First try to get with Product included
                var usages = await _unitOfWork.InternalProductUsage.GetAllWithProductAsync();
                var usage = usages.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

                if (usage == null)
                    return null;

                return new InternalUsageDetailsVM
                {
                    Id = usage.Id,
                    ProductId = usage.ProductId,
                    ProductName = usage.Product?.Name ?? "غير معروف",
                    ProductType = (int)(usage.Product?.ProductType ?? 0),
                    Quantity = usage.Quantity,
                    Weight = usage.Weight,
                    UnitPrice = usage.UnitPrice,
                    TotalCost = usage.TotalCost,
                    UsageCategory = usage.UsageCategory,
                    UsageDate = usage.UsageDate,
                    ReferenceNumber = usage.ReferenceNumber,
                    Notes = usage.Notes,
                    StockQuantityBefore = usage.StockQuantityBefore,
                    StockQuantityAfter = usage.StockQuantityAfter,
                    CreatedAt = usage.CreatedAt,
                    UpdatedAt = usage.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUsageDetailsAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<InternalUsageDetailsVM>> GetAllInternalUsageAsync()
        {
            var usages = await _unitOfWork.InternalProductUsage.GetAllWithProductAsync();
            return usages
                .Where(u => !u.IsDeleted)
                .Select(u => MapToViewModel(u))
                .OrderByDescending(u => u.UsageDate)
                .ToList();
        }

        public async Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByProductAsync(int productId)
        {
            var usages = await _unitOfWork.InternalProductUsage.GetByProductIdAsync(productId);
            return usages
                .Where(u => !u.IsDeleted)
                .Select(u => MapToViewModel(u))
                .OrderByDescending(u => u.UsageDate)
                .ToList();
        }

        public async Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var usages = await _unitOfWork.InternalProductUsage.GetByDateRangeAsync(startDate, endDate);
            return usages
                .Where(u => !u.IsDeleted)
                .Select(u => MapToViewModel(u))
                .OrderByDescending(u => u.UsageDate)
                .ToList();
        }

        public async Task<IEnumerable<InternalUsageDetailsVM>> GetUsageByCategoryAsync(string category)
        {
            var usages = await _unitOfWork.InternalProductUsage.GetByUsageCategoryAsync(category);
            return usages
                .Where(u => !u.IsDeleted)
                .Select(u => MapToViewModel(u))
                .OrderByDescending(u => u.UsageDate)
                .ToList();
        }

        public async Task<(decimal TotalCost, decimal TotalQuantity)> GetProductUsageSummaryAsync(int productId)
        {
            var usages = await _unitOfWork.InternalProductUsage.GetByProductIdAsync(productId);
            var activeUsages = usages.Where(u => !u.IsDeleted).ToList();
            return (activeUsages.Sum(u => u.TotalCost), activeUsages.Sum(u => GetEffectiveQuantity(u.ProductType ?? 0, u.Quantity, u.Weight)));
        }

        public async Task<(decimal TotalCost, int RecordCount)> GetMonthlyUsageSummaryAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var usages = await _unitOfWork.InternalProductUsage.GetByDateRangeAsync(startDate, endDate);
            var activeUsages = usages.Where(u => !u.IsDeleted).ToList();
            return (activeUsages.Sum(u => u.TotalCost), activeUsages.Count);
        }

        private InternalUsageDetailsVM MapToViewModel(InternalProductUsage u)
        {
            return new InternalUsageDetailsVM
            {
                Id = u.Id,
                ProductId = u.ProductId,
                ProductName = u.Product?.Name ?? "غير معروف",
                ProductType = (int)(u.Product?.ProductType ?? 0),
                Quantity = u.Quantity,
                Weight = u.Weight,
                UnitPrice = u.UnitPrice,
                TotalCost = u.TotalCost,
                UsageCategory = u.UsageCategory,
                UsageDate = u.UsageDate,
                ReferenceNumber = u.ReferenceNumber,
                Notes = u.Notes,
                StockQuantityBefore = u.StockQuantityBefore,
                StockQuantityAfter = u.StockQuantityAfter,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            };
        }

        private static decimal GetEffectiveQuantity(int productType, decimal quantity, decimal weight)
        {
            return productType == (int)ProductType.Count ? quantity : weight;
        }
    }
}
