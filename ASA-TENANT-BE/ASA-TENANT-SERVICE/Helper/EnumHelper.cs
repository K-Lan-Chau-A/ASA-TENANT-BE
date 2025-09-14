using ASA_TENANT_SERVICE.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Helper
{
    public static class EnumHelper
    {
        public static PaymentMethod ParsePaymentMethod(string value)
        {
            return Enum.TryParse<PaymentMethod>(value, out var method) ? method : default;
        }

        public static PaymentMethod? ParseNullablePaymentMethod(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return Enum.TryParse<PaymentMethod>(value, out var method) ? method : (PaymentMethod?)null;
        }
    }

}
