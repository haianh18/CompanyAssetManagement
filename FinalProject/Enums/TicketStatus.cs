using System.ComponentModel;

namespace FinalProject.Enums
{
    public enum TicketStatus
    {
        [Description("Đã được duyệt")]
        Approved,
        [Description("Từ chối")]
        Rejected,
        [Description("Chờ duyệt")]
        Pending
    }
}
