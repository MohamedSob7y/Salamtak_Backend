using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Payments
{
    public class PaymentStartResponseDto
    {
        public Guid PaymentId { get; set; }

        public Guid AppointmentId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        public string PaymentUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
