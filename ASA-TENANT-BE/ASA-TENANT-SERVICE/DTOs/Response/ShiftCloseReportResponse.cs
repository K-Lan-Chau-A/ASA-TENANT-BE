using System;
using System.Collections.Generic;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ShiftCloseReportResponse
    {
        public long ShiftId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? UserId { get; set; }
        public decimal? OpeningCash { get; set; }

        public decimal GrossRevenueTotal { get; set; }
        public decimal ProgramDiscountsTotal { get; set; }
        public decimal ManualDiscountAmount { get; set; }
        public int OrderCount { get; set; }
        public int GuestCount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal Aov { get; set; }

        public decimal TheoreticalCashInDrawer { get; set; }

        public List<VoucherUsageItem> VoucherCounts { get; set; } = new();
        public List<PaymentMethodItem> PaymentMethods { get; set; } = new();
        public List<ProductGroupItem> ProductGroups { get; set; } = new();
    }

    public class VoucherUsageItem
    {
        public long VoucherId { get; set; }
        public string VoucherName { get; set; }
        public int Count { get; set; }
    }

    public class PaymentMethodItem
    {
        public string Method { get; set; } // keep string as requested
        public int OrderCount { get; set; }
        public decimal Amount { get; set; }
    }

    public class ProductGroupItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}


