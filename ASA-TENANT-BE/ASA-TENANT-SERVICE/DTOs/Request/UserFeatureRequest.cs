using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class UserFeatureRequest
    {
        public long UserId { get; set; }
        public List<FeatureDto> Features { get; set; } = new();
    }
    public class FeatureDto
    {
        public long FeatureId { get; set; }
        public string FeatureName { get; set; }
        public bool? IsEnabled { get; set; }

    }
    public class UserFeatureGetRequest
    {
        public long? UserFeatureId { get; set; }

        public long? UserId { get; set; }

        public long? FeatureId { get; set; }

        public string? FeatureName { get; set; }

        public bool? IsEnabled { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
    public class UserFeatureUpdateRequest
    {
        public long UserId { get; set; }
        public List<UserFeatureItem> Features { get; set; } = new List<UserFeatureItem>();
    }

    public class UserFeatureItem
    {
        public long FeatureId { get; set; }
        public bool IsEnable { get; set; }
    }
}
