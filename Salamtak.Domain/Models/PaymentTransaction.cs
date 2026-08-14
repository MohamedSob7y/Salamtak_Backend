using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid PatientId { get; set; }

        public Guid AppointmentId { get; set; }

        public PaymentProvider Provider { get; set; } = PaymentProvider.Paymob;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string? PaymobOrderId { get; set; }

        public string? PaymobTransactionId { get; set; }

        public string? PaymobPaymentKey { get; set; }

        public string? PaymentUrl { get; set; }

        public string? FailureReason { get; set; }

        public DateTime? PaidAt { get; set; }

        public Patient Patient { get; set; } = null!;

        public Appointment Appointment { get; set; } = null!;
    }
}
