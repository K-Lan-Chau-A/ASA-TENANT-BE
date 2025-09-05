using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class FcmResponse
    {
        public long FcmId { get; set; }

        public string FcmToken { get; set; }

        public long? UserId { get; set; }
    }
}
