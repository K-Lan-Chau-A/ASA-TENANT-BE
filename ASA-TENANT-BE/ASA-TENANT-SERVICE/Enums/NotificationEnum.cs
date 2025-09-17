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
        [Description("Cảnh báo")]
        Warning = 1,
        [Description("Thông tin")]
        Information = 2
    }
}
