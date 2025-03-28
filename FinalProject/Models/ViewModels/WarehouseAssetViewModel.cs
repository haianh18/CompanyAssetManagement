namespace FinalProject.Models.ViewModels
{
    public class WarehouseAssetViewModel
    {
        public Asset Asset { get; set; }
        public WarehouseAsset WarehouseAsset { get; set; }
        public string WarehouseName { get; set; }
        public int TotalGoodQuantity { get; set; }
        public int TotalBrokenQuantity { get; set; }
        public int TotalFixingQuantity { get; set; }
        public int TotalDisposedQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
