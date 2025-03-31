namespace FinalProject.Models.ViewModels
{
    public class WarehouseManagerDashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int TotalCategories { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalDisposedAssets { get; set; }
        public int TotalPendingBorrowRequests { get; set; }
        public int ActiveAssets { get; set; }
        public int BrokenAssets { get; set; }
        public int FixingAssets { get; set; }
        public int ManagerReturnRequestsCount { get; set; }
    }
}
