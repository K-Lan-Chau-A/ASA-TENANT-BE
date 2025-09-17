using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum InventoryTransactionType
    {
        [Description("Bán hàng")]
        Sale = 1,

        [Description("Nhập hàng")]
        Import = 2
    }
}
