using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class RankResponse
    {
        public int RankId { get; set; }
        public string RankName { get; set; }
        public double? Benefit { get; set; }
        public double? Threshold { get; set; }
        public long ShopId { get; set; }
    }
}
