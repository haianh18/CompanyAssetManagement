using FinalProject.Enums;
using FinalProject.Models;

namespace FinalProject.Services.Interfaces
{
    public interface IHandoverService
    {
        // Quản lý trạng thái phiếu bàn giao
        Task UpdateHandoverTicketStatus(int handoverTicketId, bool isActive, DateTime? actualEndDate);

        // Quản lý số lượng tài sản
        Task UpdateWarehouseAssetQuantitiesForHandover(int warehouseAssetId, int quantityChange, bool isReturn, AssetStatus status);

        // Lấy thông tin chi tiết phiếu bàn giao
        Task<HandoverTicket> GetHandoverTicketWithDetailsAsync(int handoverTicketId);

        // Kiểm tra điều kiện bàn giao/trả
        Task<bool> ValidateHandoverOperationAsync(int warehouseAssetId, int quantity, bool isReturn);

        // Xử lý sự kiện bàn giao/trả
        Task ProcessHandoverEventAsync(HandoverTicket handoverTicket, string eventType, string note);

        // Kiểm tra tài sản có sẵn để bàn giao
        Task<bool> IsAssetAvailableForHandoverAsync(int warehouseAssetId, int quantity);

        // Lấy danh sách phiếu bàn giao theo nhân viên
        Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployeeAsync(int employeeId);

        // Lấy danh sách lịch sử trả tài sản
        Task<IEnumerable<HandoverReturn>> GetHandoverReturnHistoryAsync(int handoverTicketId);
    }
}
