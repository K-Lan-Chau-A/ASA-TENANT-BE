using System.ComponentModel.DataAnnotations;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class BroadcastNotificationRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public short Type { get; set; }
    }
}
