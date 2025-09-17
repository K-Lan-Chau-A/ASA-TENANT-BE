using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum ReportEnum
    {
        [Description("Tuần")]
        Weekly = 1,

        [Description("Tháng")]
        Monthly = 2
    }
}
