using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IPaymobClient
    {
        Task<string> GetAuthTokenAsync();

        Task<string> CreateOrderAsync(
            string authToken,
            int amountCents,
            string merchantOrderId);

        Task<string> CreatePaymentKeyAsync(
            string authToken,
            string paymobOrderId,
            int amountCents,
            string currency,
            int integrationId,
            string customerEmail,
            string customerPhone,
            string customerName);

        string BuildIframeUrl(
            string paymentKey,
            int iframeId);
    }
}
