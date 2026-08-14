using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Payments;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("appointments/start")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ApiResponse<PaymentStartResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> StartAppointmentPayment(
            [FromBody] StartAppointmentPaymentDto dto)
        {
            var patientUserId = GetCurrentUserId();

            var response =
                await _paymentService.StartAppointmentPaymentAsync(
                    patientUserId,
                    dto);

            return Ok(response);
        }

        [HttpGet("{paymentId:guid}")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(ApiResponse<PaymentTransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPayment(
            Guid paymentId)
        {
            var patientUserId = GetCurrentUserId();

            var response =
                await _paymentService.GetPaymentByIdAsync(
                    patientUserId,
                    paymentId);

            return Ok(response);
        }

        [HttpPost("paymob/webhook")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PaymobWebhook(
            [FromBody] PaymobWebhookDto dto,
            [FromQuery] string? hmac)
        {
            var response =
                await _paymentService.HandlePaymobWebhookAsync(
                    dto,
                    hmac);

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid or missing user id in token.");
            }

            return currentUserId;
        }
    }
}
