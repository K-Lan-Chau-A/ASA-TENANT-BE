using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class CustomerRequest
    {       
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Rankid { get; set; }
        public string? Email { get; set; }
        public decimal? Spent { get; set; }
        public short? Gender { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Avatar { get; set; }
        public short? Status { get; set; }
        public long ShopId { get; set; }
    }

    public class CustomerGetRequest
    {
        public long? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Rankid { get; set; }
        public decimal? Spent { get; set; }
        public long? ShopId { get; set; }
    }
}
