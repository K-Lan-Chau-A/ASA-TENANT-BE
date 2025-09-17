using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum PromotionStatus
    {
        [Description("Ngưng hoạt động")]
        Inactive = 0,
        [Description("Đang hoạt động")]
        Active = 1
    }
    public enum PromotionType
    {
        [Description("Tiền")]
        Money = 1,

        [Description("%")]
        Percentage = 2
    }
}
