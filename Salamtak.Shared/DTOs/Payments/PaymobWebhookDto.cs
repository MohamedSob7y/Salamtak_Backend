using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Payments
{

    public class PaymobWebhookDto
    {
        public PaymobWebhookObjectDto Obj { get; set; } = new();
    }

    public class PaymobWebhookObjectDto
    {
        public long Id { get; set; }

        public bool Success { get; set; }

        public bool Pending { get; set; }

        public bool Is_Voided { get; set; }

        public bool Is_Refunded { get; set; }

        public bool Error_Occurred { get; set; }

        public int Amount_Cents { get; set; }

        public PaymobOrderDto Order { get; set; } = new();
    }

    public class PaymobOrderDto
    {
        public long Id { get; set; }
    }
}
