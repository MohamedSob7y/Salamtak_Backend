using Microsoft.Extensions.Options;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.services.Payments;
using Salamtak.Shared.DTOs.Payments;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymobClient _paymobClient;
        private readonly PaymobOptions _options;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymobClient paymobClient,
            IOptions<PaymobOptions> options)
        {
            _unitOfWork = unitOfWork;
            _paymobClient = paymobClient;
            _options = options.Value;
        }

        public async Task<ApiResponse<PaymentStartResponseDto>>
            StartAppointmentPaymentAsync(
                Guid patientUserId,
                StartAppointmentPaymentDto dto)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId &&
                    !p.IsDeleted);

            if (patient is null)
                throw new NotFoundException("Patient profile not found.");

            var patientUser = await _unitOfWork
                .Repository<User>()
                .FirstOrDefaultAsync(u =>
                    u.Id == patient.UserId &&
                    !u.IsDeleted);

            if (patientUser is null)
                throw new NotFoundException("Patient user not found.");

            var appointment = await _unitOfWork
                .Repository<Appointment>()
                .GetByIdAsync(dto.AppointmentId);

            if (appointment is null || appointment.IsDeleted)
                throw new NotFoundException("Appointment not found.");

            if (appointment.PatientId != patient.Id)
                throw new ForbiddenException("You cannot pay for this appointment.");

            if (appointment.Status == AppointmentStatus.Cancelled)
                throw new BadRequestException("Cancelled appointment cannot be paid.");

            var existingPaidPayment = await _unitOfWork
                .Repository<PaymentTransaction>()
                .AnyAsync(p =>
                    p.AppointmentId == appointment.Id &&
                    p.Status == PaymentStatus.Paid &&
                    !p.IsDeleted);

            if (existingPaidPayment)
                throw new ConflictException("This appointment is already paid.");

            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .GetByIdAsync(appointment.DoctorId);

            if (doctor is null || doctor.IsDeleted)
                throw new NotFoundException("Doctor not found.");

            var amount = doctor.ConsultationFee ?? 0;

            if (amount <= 0)
                throw new BadRequestException("Invalid appointment payment amount.");

            var amountCents = ToCents(amount);

            var authToken = await _paymobClient.GetAuthTokenAsync();

            var merchantOrderId = $"SAL-{appointment.Id:N}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var paymobOrderId = await _paymobClient.CreateOrderAsync(
                authToken,
                amountCents,
                merchantOrderId);

            var paymentKey = await _paymobClient.CreatePaymentKeyAsync(
                authToken,
                paymobOrderId,
                amountCents,
                _options.Currency,
                _options.IntegrationId,
                patientUser.Email,
                patientUser.PhoneNumber,
                patientUser.FullName);

            var paymentUrl = _paymobClient.BuildIframeUrl(
                paymentKey,
                _options.IframeId);

            var payment = new PaymentTransaction
            {
                PatientId = patient.Id,
                AppointmentId = appointment.Id,
                Provider = PaymentProvider.Paymob,
                Status = PaymentStatus.Pending,
                Amount = amount,
                Currency = _options.Currency,
                PaymobOrderId = paymobOrderId,
                PaymobPaymentKey = paymentKey,
                PaymentUrl = paymentUrl
            };

            await _unitOfWork
                .Repository<PaymentTransaction>()
                .AddAsync(payment);

            await _unitOfWork.SaveChangesAsync();

            var result = new PaymentStartResponseDto
            {
                PaymentId = payment.Id,
                AppointmentId = appointment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentUrl = payment.PaymentUrl!,
                Status = payment.Status.ToString()
            };

            return ApiResponse<PaymentStartResponseDto>.Ok(
                result,
                "Payment session created successfully.");
        }

        public async Task<ApiResponse<PaymentTransactionDto>>
            GetPaymentByIdAsync(
                Guid patientUserId,
                Guid paymentId)
        {
            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId &&
                    !p.IsDeleted);

            if (patient is null)
                throw new NotFoundException("Patient profile not found.");

            var payment = await _unitOfWork
                .Repository<PaymentTransaction>()
                .GetByIdAsync(paymentId);

            if (payment is null || payment.IsDeleted)
                throw new NotFoundException("Payment not found.");

            if (payment.PatientId != patient.Id)
                throw new ForbiddenException("You cannot view this payment.");

            return ApiResponse<PaymentTransactionDto>.Ok(
                MapPayment(payment),
                "Payment retrieved successfully.");
        }

        public async Task<ApiResponse>
            HandlePaymobWebhookAsync(
                PaymobWebhookDto dto,
                string? hmac)
        {
            var paymobOrderId = dto.Obj.Order.Id.ToString();

            var payment = await _unitOfWork
                .Repository<PaymentTransaction>()
                .FirstOrDefaultAsync(p =>
                    p.PaymobOrderId == paymobOrderId &&
                    !p.IsDeleted);

            if (payment is null)
                throw new NotFoundException("Payment transaction not found.");

            payment.PaymobTransactionId = dto.Obj.Id.ToString();

            if (dto.Obj.Success &&
                !dto.Obj.Pending &&
                !dto.Obj.Error_Occurred &&
                !dto.Obj.Is_Refunded &&
                !dto.Obj.Is_Voided)
            {
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;

                var appointment = await _unitOfWork
                    .Repository<Appointment>()
                    .GetByIdAsync(payment.AppointmentId);

                if (appointment is not null &&
                    !appointment.IsDeleted &&
                    appointment.Status != AppointmentStatus.Cancelled)
                {
                    appointment.Status = AppointmentStatus.Confirmed;

                    _unitOfWork
                        .Repository<Appointment>()
                        .Update(appointment);
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Paymob payment failed or was not successful.";
            }

            _unitOfWork
                .Repository<PaymentTransaction>()
                .Update(payment);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Paymob webhook handled successfully.");
        }

        private static int ToCents(decimal amount)
        {
            return (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
        }

        private static PaymentTransactionDto MapPayment(
            PaymentTransaction payment)
        {
            return new PaymentTransactionDto
            {
                PaymentId = payment.Id,
                AppointmentId = payment.AppointmentId,
                PatientId = payment.PatientId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Provider = payment.Provider.ToString(),
                Status = payment.Status.ToString(),
                PaymobOrderId = payment.PaymobOrderId,
                PaymobTransactionId = payment.PaymobTransactionId,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }
    }
}
