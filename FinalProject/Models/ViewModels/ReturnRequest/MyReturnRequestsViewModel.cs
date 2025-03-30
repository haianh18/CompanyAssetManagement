namespace FinalProject.Models.ViewModels.ReturnRequest
{
    public class MyReturnRequestsViewModel
    {
        public IEnumerable<ReturnTicket> AllReturnRequests { get; set; } = new List<ReturnTicket>();
        public IEnumerable<ReturnTicket> PendingReturnRequests { get; set; } = new List<ReturnTicket>();
        public IEnumerable<ReturnTicket> ApprovedReturnRequests { get; set; } = new List<ReturnTicket>();
        public IEnumerable<ReturnTicket> RejectedReturnRequests { get; set; } = new List<ReturnTicket>();
    }
}
