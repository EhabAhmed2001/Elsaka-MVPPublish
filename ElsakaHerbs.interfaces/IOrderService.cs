using ElsakaHerbs.ViewModels.Checkout;
using ElsakaHerbs.ViewModels.Orders;

namespace ElsakaHerbs.interfaces;

public interface IOrderService
{
    Task<(bool Success, string? ErrorMessage, OrderConfirmationViewModel? Order)> PlaceOrderAsync(OrderCheckoutViewModel checkoutModel);
    Task<List<OrderListViewModel>> GetAllOrdersAsync(int? statusId = null, string? search = null);
    Task<OrderPagedResultViewModel> GetOrdersPagedAsync(string? phone = null, string? orderNumber = null, int? statusId = null, int pageNumber = 1, int pageSize = 10);
    Task<List<OrderStatusViewModel>> GetAllStatusesAsync();
    Task<OrderStatisticsViewModel> GetOrderStatisticsAsync();
    Task<OrderDetailViewModel?> GetOrderByIdAsync(int id);
    Task<OrderDetailViewModel?> GetOrderByNumberAsync(string orderNumber);
    Task<bool> UpdateOrderStatusAsync(int orderId, int newStatusId);
    Task<int> GetNewOrdersCountAsync();
    Task<int> GetTotalOrdersCountAsync();
}
