using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Payments
{
    public class PaymobOptions
    {
        public string BaseUrl { get; set; } = "https://accept.paymob.com";

        public string ApiKey { get; set; } = string.Empty;

        public int IntegrationId { get; set; }

        public int IframeId { get; set; }

        public string HmacSecret { get; set; } = string.Empty;

        public string Currency { get; set; } = "EGP";
    }
}
