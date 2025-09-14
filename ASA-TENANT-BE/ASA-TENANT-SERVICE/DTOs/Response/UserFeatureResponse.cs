using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class UserFeatureResponse
    {
        public long UserFeatureId { get; set; }

        public long? UserId { get; set; }

        public long FeatureId { get; set; }

        public string FeatureName { get; set; }

        public bool? IsEnabled { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string Username { get; set; }

        public long? ShopId { get; set; }

        public short? Role { get; set; }
    }
}
