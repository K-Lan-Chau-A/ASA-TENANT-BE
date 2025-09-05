using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ShiftRequest
    {
        public long? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public short? Status { get; set; }

        public decimal? Revenue { get; set; }

        public decimal? OpeningCash { get; set; }

        public long? ShopId { get; set; }
    }
    public class ShiftGetRequest
    {
        public long? ShiftId { get; set; }

        public long? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public short? Status { get; set; }

        public decimal? Revenue { get; set; }

        public decimal? OpeningCash { get; set; }

        public long? ShopId { get; set; }
    }
}
