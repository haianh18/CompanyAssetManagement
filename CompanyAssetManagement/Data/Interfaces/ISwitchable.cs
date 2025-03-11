using CompanyAssetManagement.Data.Enums;

namespace CompanyAssetManagement.Data.Interfaces
{
    public interface ISwitchable
    {
        ActiveStatus ActiveStatus { get; set; }
    }
}
