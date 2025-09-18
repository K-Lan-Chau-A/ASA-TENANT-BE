using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum ShiftStatus
    {
        [Description("Đang mở")]
        Open = 1,

        [Description("Đã đóng")]
        Closed = 2
    }
}
