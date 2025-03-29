namespace FinalProject.Models.ViewModels.Handover
{
    public class HandoverTicketsViewModel
    {

        public IEnumerable<HandoverTicket> AllHandovers { get; set; } = new List<HandoverTicket>();
        public IEnumerable<HandoverTicket> ActiveHandovers { get; set; } = new List<HandoverTicket>();
        public IEnumerable<HandoverTicket> InactiveHandovers { get; set; } = new List<HandoverTicket>();
    }
}
