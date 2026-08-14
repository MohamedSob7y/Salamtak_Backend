using Salamtak.Shared.DTOs.Payments;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentStartResponseDto>>
            StartAppointmentPaymentAsync(
                Guid patientUserId,
                StartAppointmentPaymentDto dto);

        Task<ApiResponse<PaymentTransactionDto>>
            GetPaymentByIdAsync(
                Guid patientUserId,
                Guid paymentId);

        Task<ApiResponse>
            HandlePaymobWebhookAsync(
                PaymobWebhookDto dto,
                string? hmac);
    }
}
