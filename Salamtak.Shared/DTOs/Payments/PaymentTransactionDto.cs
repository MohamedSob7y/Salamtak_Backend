using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Payments
{
    public class PaymentTransactionDto
    {
        public Guid PaymentId { get; set; }

        public Guid AppointmentId { get; set; }

        public Guid PatientId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? PaymobOrderId { get; set; }

        public string? PaymobTransactionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
