using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum UserRole
    {
        [Description("Quản lý")]
        Admin = 1,
        [Description("Nhân viên")]
        Staff = 2
    }
    public enum UserStatus
    {
        [Description("Ngưng hoạt động")]
        Inactive = 0,
        [Description("Đang hoạt động")]
        Active = 1
    }
}
