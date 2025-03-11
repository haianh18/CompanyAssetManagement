using System.ComponentModel;

namespace CompanyAssetManagement.Data.Enums
{
    public enum AssetStatus
    {
        [Description("Tốt")]
        GOOD,

        [Description("Hỏng")]
        BROKEN,

        [Description("Đang sửa chữa")]
        FIXING,

        [Description("Đã thanh lý")]
        DISPOSED
    }
}
