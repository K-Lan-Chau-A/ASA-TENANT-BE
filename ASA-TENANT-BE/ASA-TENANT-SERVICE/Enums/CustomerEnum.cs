using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum CustomerStatus
    {
        [Description("Không hoạt động")]
        Inactive = 0,

        [Description("Đang hoạt động")]
        Active = 1
    }

    public enum Gender
    {
        [Description("Nam")]
        Male = 1,

        [Description("Nữ")]
        Female = 2,

        [Description("Không xác định")]
        Unknown = 3
    }
    public enum CustomerRank
    {
        [Description("Đồng")]
        Bronze = 1,

        [Description("Bạc")]
        Silver = 2,

        [Description("Vàng")]
        Gold = 3,

        [Description("Bạch Kim")]
        Platinum = 4,

        [Description("Kim Cương")]
        Diamond = 5
    }
}
