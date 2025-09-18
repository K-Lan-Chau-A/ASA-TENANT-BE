using ASA_TENANT_SERVICE.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class UserCreateRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        //[EnumDataType(typeof(UserRole))]
        //public UserRole Role { get; set; }
        public string? Avatar { get; set; }
    }

    public class UserUpdateRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }
        public string? Avatar { get; set; }
    }



    public class UserGetRequest
    {
        public long? UserId { get; set; }
        public string? Username { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        public short? Role { get; set; }

    }
}
