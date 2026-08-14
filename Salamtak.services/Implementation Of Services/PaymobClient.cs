using Microsoft.Extensions.Options;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class PaymobClient : IPaymobClient
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobOptions _options;

        public PaymobClient(
            HttpClient httpClient,
            IOptions<PaymobOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/tokens",
                new
                {
                    api_key = _options.ApiKey
                });

            var json =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    $"Paymob auth failed: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            return document.RootElement
                .GetProperty("token")
                .GetString()
                ?? throw new BadRequestException(
                    "Paymob auth token missing.");
        }

        public async Task<string> CreateOrderAsync(
            string authToken,
            int amountCents,
            string merchantOrderId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/ecommerce/orders",
                new
                {
                    auth_token = authToken,
                    delivery_needed = false,
                    amount_cents = amountCents,
                    currency = _options.Currency,
                    merchant_order_id = merchantOrderId,
                    items = Array.Empty<object>()
                });

            var json =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    $"Paymob order creation failed: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            return document.RootElement
                .GetProperty("id")
                .GetInt64()
                .ToString();
        }

        public async Task<string> CreatePaymentKeyAsync(
            string authToken,
            string paymobOrderId,
            int amountCents,
            string currency,
            int integrationId,
            string customerEmail,
            string customerPhone,
            string customerName)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = amountCents,
                    expiration = 3600,
                    order_id = paymobOrderId,
                    billing_data = new
                    {
                        apartment = "NA",
                        email = customerEmail,
                        floor = "NA",
                        first_name = GetFirstName(customerName),
                        street = "NA",
                        building = "NA",
                        phone_number = customerPhone,
                        shipping_method = "NA",
                        postal_code = "NA",
                        city = "Cairo",
                        country = "EG",
                        last_name = GetLastName(customerName),
                        state = "Cairo"
                    },
                    currency = currency,
                    integration_id = integrationId,
                    lock_order_when_paid = true
                });

            var json =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    $"Paymob payment key failed: {json}");
            }

            using var document =
                JsonDocument.Parse(json);

            return document.RootElement
                .GetProperty("token")
                .GetString()
                ?? throw new BadRequestException(
                    "Paymob payment key missing.");
        }

        public string BuildIframeUrl(
            string paymentKey,
            int iframeId)
        {
            return
                $"{_options.BaseUrl.TrimEnd('/')}" +
                $"/api/acceptance/iframes/{iframeId}" +
                $"?payment_token={paymentKey}";
        }

        private static string GetFirstName(
            string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "Customer";
            }

            return fullName
                .Trim()
                .Split(' ',
                    StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private static string GetLastName(
            string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "Customer";
            }

            var parts = fullName
                .Trim()
                .Split(' ',
                    StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 1
                ? parts[^1]
                : "Customer";
        }
    }
}
