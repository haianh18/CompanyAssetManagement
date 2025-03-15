using FinalProject.Enums;

namespace FinalProject.Models.ViewModels
{
    public class AssetSummaryReportViewModel
    {
        public List<Asset> Assets { get; set; }
        public List<AssetCategory> Categories { get; set; }
        public List<Warehouse> Warehouses { get; set; }
        public double TotalValue { get; set; }
        public Dictionary<AssetStatus, int> AssetCountByStatus { get; set; }
    }
}
