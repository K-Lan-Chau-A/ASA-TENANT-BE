using ASA_TENANT_SERVICE.Enums;
using System;
using Microsoft.AspNetCore.Http;
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
        public string? FullName { get; set; }

        [RegularExpression(@"^(0\d{9}|\+84\d{9})$", ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? CitizenIdNumber { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
        //[EnumDataType(typeof(UserRole))]
        //public UserRole Role { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
    public class UserAdminCreateRequest
    {
        public string? Username { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        //[EnumDataType(typeof(UserRole))]
        //public UserRole Role { get; set; }
        public string? Avatar { get; set; }
    }
    public class UserUpdateRequest
    {
        public string? Password { get; set; }
        public string? FullName { get; set; }

        [RegularExpression(@"^(0\d{9}|\+84\d{9})$", ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? CitizenIdNumber { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
        public short? Status { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }



    public class UserGetRequest
    {
        public long? UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? CitizenIdNumber { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
        public short? Status { get; set; }
        public short? Role { get; set; }

    }
}
