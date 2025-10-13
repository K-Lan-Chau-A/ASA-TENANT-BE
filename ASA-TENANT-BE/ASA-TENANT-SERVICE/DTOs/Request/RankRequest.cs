using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class RankGetRequest
    {
        public int? RankId { get; set; }

        public string? RankName { get; set; }

        public double? Benefit { get; set; }

        public double? Threshold { get; set; }
        [Required]
        public long ShopId { get; set; }
    }
    public class RankRequest
    {
        [Required]
        public string RankName { get; set; }
        public double? Benefit { get; set; }
        public double? Threshold { get; set; }
        [Required]
        public long ShopId { get; set; }
    }
}
