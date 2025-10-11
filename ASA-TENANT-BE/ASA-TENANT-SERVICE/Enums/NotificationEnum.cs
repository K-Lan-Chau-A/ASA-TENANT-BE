using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum NotificationType
    {
        [Description("Mặc định")]
        Default = 0,
        [Description("Cảnh báo")]
        Warning = 1,
        [Description("Ưu đãi")]
        Promotion = 2,
        [Description("Gợi ý")]
        Suggestion = 3,
        [Description("Thành công")]
        Success = 4
    }
}
