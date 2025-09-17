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
        public static PaymentMethodEnum ParsePaymentMethod(string value)
        {
            return Enum.TryParse<PaymentMethodEnum>(value, out var method) ? method : default;
        }

        public static PaymentMethodEnum? ParseNullablePaymentMethod(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return Enum.TryParse<PaymentMethodEnum>(value, out var method) ? method : (PaymentMethodEnum?)null;
        }
    }

}
