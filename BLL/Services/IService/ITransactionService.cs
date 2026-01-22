using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.IService
{
    public interface ITransactionService
    {
        Task LogProductTransactionAsync(
            int productId,
            int invoiceId,
            string type,
            decimal quantityBefore,
            decimal quantityChanged,
            decimal weightChanged,
            decimal quantityAfter,
            decimal unitPrice,
            string referenceNumber,
            string notes,
            int? productType = null);

        Task LogCustomerTransactionAsync(
            int customerId,
            int invoiceId,
            string type,
            decimal balanceBefore,
            decimal amountChanged,
            decimal balanceAfter,
            string description,
            string referenceNumber);

        Task LogSupplierTransactionAsync(
            int supplierId,
            int invoiceId,
            string type,
            decimal balanceBefore,
            decimal amountChanged,
            decimal balanceAfter,
            string description,
            string referenceNumber);

        Task<bool> RevertInvoiceTransactionsAsync(int invoiceId);
    }
}
