using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;

namespace FinalProject.Services
{
    public class HandoverService : IHandoverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWarehouseAssetService _warehouseAssetService;

        public HandoverService(
            IUnitOfWork unitOfWork,
            IWarehouseAssetService warehouseAssetService)
        {
            _unitOfWork = unitOfWork;
            _warehouseAssetService = warehouseAssetService;
        }

        public async Task UpdateHandoverTicketStatus(int handoverTicketId, bool isActive, DateTime? actualEndDate)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(handoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Handover ticket not found");

            handoverTicket.IsActive = isActive;
            handoverTicket.ActualEndDate = actualEndDate;
            handoverTicket.DateModified = DateTime.Now;

            _unitOfWork.HandoverTickets.Update(handoverTicket);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateWarehouseAssetQuantitiesForHandover(int warehouseAssetId, int quantityChange, bool isReturn, AssetStatus status)
        {
            // Cập nhật số lượng tài sản trong kho
            if (isReturn)
            {
                // Trả lại tài sản - giảm số lượng đã bàn giao
                await _warehouseAssetService.UpdateHandedOverQuantityAsync(warehouseAssetId, -quantityChange);

                // Cập nhật số lượng tài sản dựa trên trạng thái khi trả
                if (status == AssetStatus.GOOD)
                {
                    // Nếu trả về trong tình trạng tốt, thêm vào số lượng tốt
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAssetId,
                        AssetStatus.GOOD,
                        AssetStatus.GOOD,
                        quantityChange);
                }
                else
                {
                    // Nếu trả về trong tình trạng khác, chuyển từ tốt sang trạng thái đó
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAssetId,
                        AssetStatus.GOOD,
                        status,
                        quantityChange);
                }
            }
            else
            {
                // Bàn giao mới - tăng số lượng đã bàn giao
                await _warehouseAssetService.UpdateHandedOverQuantityAsync(warehouseAssetId, quantityChange);
            }
        }

        public async Task<HandoverTicket> GetHandoverTicketWithDetailsAsync(int handoverTicketId)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetHandoverTicketWithDetails(handoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Handover ticket not found");

            return handoverTicket;
        }

        public async Task<bool> ValidateHandoverOperationAsync(int warehouseAssetId, int quantity, bool isReturn)
        {
            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(warehouseAssetId);
            if (warehouseAsset == null)
                return false;

            if (isReturn)
            {
                // Khi trả lại, kiểm tra xem có đủ số lượng đã bàn giao không
                return (warehouseAsset.HandedOverGoodQuantity ?? 0) >= quantity;
            }
            else
            {
                // Khi bàn giao mới, kiểm tra xem có đủ số lượng tốt và có sẵn không
                int availableGood = (warehouseAsset.GoodQuantity ?? 0) -
                                   (warehouseAsset.BorrowedGoodQuantity ?? 0) -
                                   (warehouseAsset.HandedOverGoodQuantity ?? 0);
                return availableGood >= quantity;
            }
        }

        public async Task ProcessHandoverEventAsync(HandoverTicket handoverTicket, string eventType, string note)
        {
            // Ghi lại lịch sử sự kiện
            var eventLog = new Dictionary<string, object>
            {
                { "HandoverTicketId", handoverTicket.Id },
                { "EventType", eventType },
                { "Timestamp", DateTime.Now },
                { "Note", note },
                { "UserId", handoverTicket.HandoverById }
            };

            // Trong một hệ thống thực tế, bạn có thể lưu lịch sử sự kiện vào cơ sở dữ liệu
            // Ví dụ: await _unitOfWork.HandoverEventLogs.AddAsync(eventLog);

            // Cập nhật handover ticket
            handoverTicket.DateModified = DateTime.Now;
            handoverTicket.Note = string.IsNullOrEmpty(handoverTicket.Note)
                ? note
                : $"{handoverTicket.Note}\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {eventType}: {note}";

            _unitOfWork.HandoverTickets.Update(handoverTicket);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsAssetAvailableForHandoverAsync(int warehouseAssetId, int quantity)
        {
            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(warehouseAssetId);
            if (warehouseAsset == null)
                return false;

            int availableQuantity = (warehouseAsset.GoodQuantity ?? 0) -
                                   (warehouseAsset.BorrowedGoodQuantity ?? 0) -
                                   (warehouseAsset.HandedOverGoodQuantity ?? 0);

            return availableQuantity >= quantity;
        }

        public async Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployeeAsync(int employeeId)
        {
            return await _unitOfWork.HandoverTickets.GetActiveHandoversByEmployee(employeeId);
        }

        public async Task<IEnumerable<HandoverReturn>> GetHandoverReturnHistoryAsync(int handoverTicketId)
        {
            // Trong một hệ thống thực tế, bạn có thể có một bảng HandoverReturn để lưu lịch sử trả
            // Ví dụ: return await _unitOfWork.HandoverReturns.GetByHandoverTicketIdAsync(handoverTicketId);

            // Giả định không có bảng HandoverReturn cụ thể
            return new List<HandoverReturn>();
        }
    }
}
