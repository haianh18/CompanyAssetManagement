namespace FinalProject.Models.ViewModels.BorrowRequest
{
    public class MyBorrowRequestViewModel
    {
        public IEnumerable<BorrowTicket> AllRequests { get; set; } = new List<BorrowTicket>();
        public IEnumerable<BorrowTicket> PendingRequests { get; set; } = new List<BorrowTicket>();
        public IEnumerable<BorrowTicket> ApprovedRequests { get; set; } = new List<BorrowTicket>();
        public IEnumerable<BorrowTicket> RejectedRequests { get; set; } = new List<BorrowTicket>();
        public IEnumerable<BorrowTicket> ReturnedRequests { get; set; } = new List<BorrowTicket>();
        public IEnumerable<BorrowTicket> OverdueRequests { get; set; } = new List<BorrowTicket>();
    }
}
