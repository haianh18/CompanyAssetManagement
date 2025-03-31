using FinalProject.Models.Base;

namespace FinalProject.Models
{
    public class Notification : EntityBase
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // success, error, warning, info
        public string RelatedLink { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation property
        public virtual AppUser User { get; set; }
    }
}
