using FinalProject.Enums;
using FinalProject.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    // ViewModel cho việc hiển thị tài sản trên trang danh sách
    public class AssetViewModel
    {
        public Asset Asset { get; set; }
        public int TotalQuantity { get; set; }
        public int BorrowedQuantity { get; set; }
        public int HandedOverQuantity { get; set; }

        public int AvailableQuantity => TotalQuantity - BorrowedQuantity - HandedOverQuantity;
    }

    // ViewModel chi tiết để hiển thị thông tin đầy đủ của tài sản
    public class AssetDetailViewModel
    {
        public Asset Asset { get; set; }
        public List<WarehouseAsset> WarehouseAssets { get; set; }
        public List<BorrowTicket> BorrowTickets { get; set; }
        public List<HandoverTicket> HandoverTickets { get; set; }

        public int TotalGoodQuantity { get; set; }
        public int TotalBrokenQuantity { get; set; }
        public int TotalFixingQuantity { get; set; }
        public int TotalDisposedQuantity { get; set; }
        public int TotalBorrowedQuantity { get; set; }
        public int TotalHandedOverQuantity { get; set; }

        public int TotalQuantity => TotalGoodQuantity + TotalBrokenQuantity + TotalFixingQuantity + TotalDisposedQuantity;
        public int AvailableQuantity => TotalGoodQuantity - TotalBorrowedQuantity - TotalHandedOverQuantity;
    }

    // ViewModel cho chức năng quản lý số lượng tài sản
    public class AssetQuantityViewModel
    {
        public Asset Asset { get; set; }
        public List<WarehouseAsset> WarehouseAssets { get; set; }
    }

    // ViewModel để cập nhật số lượng tài sản trong kho
    public class AssetQuantityUpdateViewModel
    {
        public int? WarehouseAssetId { get; set; }
        public int? WarehouseId { get; set; }
        public int? AssetId { get; set; }

        [Display(Name = "Số lượng tốt")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được nhỏ hơn 0")]
        public int? GoodQuantity { get; set; }

        [Display(Name = "Số lượng hỏng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được nhỏ hơn 0")]
        public int? BrokenQuantity { get; set; }

        [Display(Name = "Số lượng đang sửa")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được nhỏ hơn 0")]
        public int? FixingQuantity { get; set; }

        [Display(Name = "Số lượng đã thanh lý")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được nhỏ hơn 0")]
        public int? DisposedQuantity { get; set; }
    }

    // ViewModel để chuyển trạng thái của tài sản (ví dụ: từ hỏng -> đang sửa)
    public class AssetStatusTransferViewModel
    {
        public int WarehouseAssetId { get; set; }
        public int AssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái nguồn")]
        [Display(Name = "Từ trạng thái")]
        public AssetStatus FromStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái đích")]
        [Display(Name = "Đến trạng thái")]
        public AssetStatus ToStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }

    // ViewModel để theo dõi lịch sử thay đổi trạng thái tài sản
    public class AssetStatusChangeLogViewModel
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public AssetStatus FromStatus { get; set; }
        public AssetStatus ToStatus { get; set; }
        public int Quantity { get; set; }
        public System.DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; }
        public string Note { get; set; }
    }

    // ViewModel để hiển thị thống kê tài sản
    public class AssetStatisticsViewModel
    {
        public int TotalAssetCount { get; set; }
        public int GoodAssetCount { get; set; }
        public int BrokenAssetCount { get; set; }
        public int FixingAssetCount { get; set; }
        public int DisposedAssetCount { get; set; }
        public int BorrowedAssetCount { get; set; }
        public int HandedOverAssetCount { get; set; }
        public double TotalAssetValue { get; set; }
        public List<AssetCategoryStatisticsItem> CategoryStatistics { get; set; }
        public List<AssetLocationStatisticsItem> LocationStatistics { get; set; }
    }

    public class AssetCategoryStatisticsItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int AssetCount { get; set; }
        public double TotalValue { get; set; }
    }

    public class AssetLocationStatisticsItem
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int AssetCount { get; set; }
        public int AvailableCount { get; set; }
    }

    // ViewModel để tìm kiếm nâng cao
    public class AssetAdvancedSearchViewModel
    {
        [Display(Name = "Tên tài sản")]
        public string Name { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Kho")]
        public int? WarehouseId { get; set; }

        [Display(Name = "Trạng thái")]
        public AssetStatus? Status { get; set; }

        [Display(Name = "Giá từ")]
        public double? MinPrice { get; set; }

        [Display(Name = "Giá đến")]
        public double? MaxPrice { get; set; }

        [Display(Name = "Chỉ hiển thị có sẵn")]
        public bool OnlyAvailable { get; set; }

        [Display(Name = "Bao gồm đã xóa")]
        public bool IncludeDeleted { get; set; }
    }
}