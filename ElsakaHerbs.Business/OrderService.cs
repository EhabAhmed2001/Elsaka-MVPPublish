using ElsakaHerbs.DataAccess.Entities;
using ElsakaHerbs.interfaces;
using ElsakaHerbs.ViewModels.Checkout;
using ElsakaHerbs.ViewModels.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElsakaHerbs.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(bool Success, string? ErrorMessage, OrderConfirmationViewModel? Order)> PlaceOrderAsync(OrderCheckoutViewModel checkoutModel)
    {
        // 1. Validate items presence
        if (checkoutModel.Items == null || checkoutModel.Items.Count == 0)
        {
            return (false, "السلة فارغة", null);
        }

        // 2. Validate and retrieve governorate shipping (Rule 6.1)
        var governorate = await _unitOfWork.GovernorateShippings.Query(asNoTracking: true)
            .FirstOrDefaultAsync(g => g.Id == checkoutModel.GovernorateShippingId && g.IsActive);

        if (governorate == null)
        {
            return (false, "المحافظة المحددة غير متاحة للشحن حالياً", null);
        }

        // 3. Retrieve active packages directly from database (Zero Client Trust - Rule 6.1)
        var requestedPackageIds = checkoutModel.Items.Select(i => i.PackageId).Distinct().ToList();
        var packages = await _unitOfWork.Packages.Query(asNoTracking: false)
            .Where(p => requestedPackageIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in checkoutModel.Items)
        {
            if (!packages.ContainsKey(item.PackageId))
            {
                return (false, "إحدى الباقات المطلوبة غير متوفرة حالياً", null);
            }

            if (item.Quantity < 1)
            {
                return (false, "الكمية المطلوبة غير صالحة", null);
            }
        }

        // 4. Server-side financial calculation (Rule 6.1)
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();
        var summaryItems = new List<OrderItemSummaryViewModel>();

        foreach (var inputItem in checkoutModel.Items)
        {
            var pkg = packages[inputItem.PackageId];
            var unitPrice = pkg.Price;
            var lineTotal = unitPrice * inputItem.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                PackageId = pkg.Id,
                PackageNameSnapshot = pkg.Name,
                UnitPriceSnapshot = unitPrice,
                Quantity = inputItem.Quantity,
                LineTotal = lineTotal
            });

            summaryItems.Add(new OrderItemSummaryViewModel
            {
                PackageName = pkg.Name,
                UnitPrice = unitPrice,
                Quantity = inputItem.Quantity,
                LineTotal = lineTotal
            });
        }

        var shippingCost = governorate.ShippingCost;
        var grandTotal = subtotal + shippingCost;

        // 5. Generate unique OrderNumber: e.g. ORD-1001 or timestamp-based
        var countToday = await _unitOfWork.Orders.CountAsync();
        var orderNumber = $"ORD-{1000 + countToday + 1}";

        // Ensure uniqueness
        while (await _unitOfWork.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
        {
            orderNumber = $"ORD-{Random.Shared.Next(1000, int.MaxValue)}";
        }

        // 6. Create immutable order record
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerFirstName = checkoutModel.CustomerFirstName.Trim(),
            CustomerLastName = checkoutModel.CustomerLastName.Trim(),
            CustomerPhone = checkoutModel.CustomerPhone.Trim(),
            GovernorateShippingId = governorate.Id,
            GovernorateNameSnapshot = governorate.GovernorateName,
            Address = checkoutModel.Address.Trim(),
            Notes = string.IsNullOrWhiteSpace(checkoutModel.Notes) ? null : checkoutModel.Notes.Trim(),
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            GrandTotal = grandTotal,
            OrderStatusId = 1, // "طلب جديد" (New)
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        // 7. Increment PurchaseCount for each purchased package by quantity purchased
        foreach (var item in checkoutModel.Items)
        {
            if (packages.TryGetValue(item.PackageId, out var pkg))
            {
                pkg.PurchaseCount += item.Quantity;
            }
        }

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Order {OrderNumber} created successfully. Total: {GrandTotal}", order.OrderNumber, order.GrandTotal);

        var confirmation = new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerFullName = $"{order.CustomerFirstName} {order.CustomerLastName}".Trim(),
            CustomerPhone = order.CustomerPhone,
            GovernorateName = order.GovernorateNameSnapshot,
            Address = order.Address,
            Notes = order.Notes,
            Subtotal = order.Subtotal,
            ShippingCost = order.ShippingCost,
            GrandTotal = order.GrandTotal,
            CreatedAt = order.CreatedAt,
            Items = summaryItems
        };

        return (true, null, confirmation);
    }

    public async Task<List<OrderListViewModel>> GetAllOrdersAsync(int? statusId = null, string? search = null)
    {
        var query = _unitOfWork.Orders.Query(asNoTracking: true)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (statusId.HasValue && statusId.Value > 0)
        {
            query = query.Where(o => o.OrderStatusId == statusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(o => o.OrderNumber.Contains(s)
                                  || o.CustomerFirstName.Contains(s)
                                  || o.CustomerLastName.Contains(s)
                                  || o.CustomerPhone.Contains(s));
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerFullName = (o.CustomerFirstName + " " + o.CustomerLastName).Trim(),
                CustomerPhone = o.CustomerPhone,
                GovernorateName = o.GovernorateNameSnapshot,
                GrandTotal = o.GrandTotal,
                ItemsCount = o.OrderItems.Sum(i => i.Quantity),
                OrderStatusId = o.OrderStatusId,
                OrderStatusName = o.OrderStatus.StatusName,
                OrderStatusColor = o.OrderStatus.StatusColor,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrderPagedResultViewModel> GetOrdersPagedAsync(
        string? phone = null,
        string? orderNumber = null,
        int? statusId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _unitOfWork.Orders.Query(asNoTracking: true)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .AsQueryable();

        // 1. Partial match for Customer Phone
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var p = phone.Trim();
            query = query.Where(o => o.CustomerPhone.Contains(p));
        }

        // 2. Partial match for Order Number (stripping leading '#' if entered)
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var num = orderNumber.Trim().TrimStart('#');
            query = query.Where(o => o.OrderNumber.Contains(num));
        }

        // 3. Status filter
        if (statusId.HasValue && statusId.Value > 0)
        {
            query = query.Where(o => o.OrderStatusId == statusId.Value);
        }

        // 4. Calculate total count after all filters and before pagination
        var totalCount = await query.CountAsync();

        // 5. Enforce valid page boundaries
        if (pageSize <= 0) pageSize = 10;
        if (pageNumber <= 0) pageNumber = 1;

        // 6. Apply pagination via Skip and Take
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerFullName = (o.CustomerFirstName + " " + o.CustomerLastName).Trim(),
                CustomerPhone = o.CustomerPhone,
                GovernorateName = o.GovernorateNameSnapshot,
                GrandTotal = o.GrandTotal,
                ItemsCount = o.OrderItems.Sum(i => i.Quantity),
                OrderStatusId = o.OrderStatusId,
                OrderStatusName = o.OrderStatus.StatusName,
                OrderStatusColor = o.OrderStatus.StatusColor,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return new OrderPagedResultViewModel
        {
            Orders = orders,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<OrderStatusViewModel>> GetAllStatusesAsync()
    {
        return await _unitOfWork.OrderStatuses.Query(asNoTracking: true)
            .OrderBy(s => s.Id)
            .Select(s => new OrderStatusViewModel
            {
                Id = s.Id,
                StatusName = s.StatusName,
                StatusColor = s.StatusColor
            })
            .ToListAsync();
    }

    public async Task<OrderStatisticsViewModel> GetOrderStatisticsAsync()
    {
        var total = await _unitOfWork.Orders.CountAsync();
        var newOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderStatusId == 1 || o.OrderStatus.StatusName == "طلب جديد");
        var shippedOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderStatusId == 2 || o.OrderStatus.StatusName == "تم الشحن");
        var deliveredOrders = await _unitOfWork.Orders.CountAsync(o => o.OrderStatusId == 3 || o.OrderStatus.StatusName == "تم التسليم");

        return new OrderStatisticsViewModel
        {
            TotalOrders = total,
            NewOrders = newOrders,
            ShippedOrders = shippedOrders,
            DeliveredOrders = deliveredOrders
        };
    }

    public async Task<OrderDetailViewModel?> GetOrderByIdAsync(int id)
    {
        var o = await _unitOfWork.Orders.Query(asNoTracking: true)
            .Include(x => x.OrderStatus)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null) return null;

        return MapToDetailViewModel(o);
    }

    public async Task<OrderDetailViewModel?> GetOrderByNumberAsync(string orderNumber)
    {
        var o = await _unitOfWork.Orders.Query(asNoTracking: true)
            .Include(x => x.OrderStatus)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber.Trim());

        if (o == null) return null;

        return MapToDetailViewModel(o);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int newStatusId)
    {
        // Validate that status exists in OrderStatus table (Requirement 2)
        var statusExists = await _unitOfWork.OrderStatuses.AnyAsync(s => s.Id == newStatusId);
        if (!statusExists)
            return false;

        var order = await _unitOfWork.Orders.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return false;

        order.OrderStatusId = newStatusId;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetNewOrdersCountAsync()
    {
        return await _unitOfWork.Orders.CountAsync(o => o.OrderStatusId == 1 || o.OrderStatus.StatusName == "طلب جديد");
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _unitOfWork.Orders.CountAsync();
    }

    private static OrderDetailViewModel MapToDetailViewModel(Order o)
    {
        return new OrderDetailViewModel
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerFirstName = o.CustomerFirstName,
            CustomerLastName = o.CustomerLastName,
            CustomerPhone = o.CustomerPhone,
            GovernorateShippingId = o.GovernorateShippingId,
            GovernorateNameSnapshot = o.GovernorateNameSnapshot,
            Address = o.Address,
            Notes = o.Notes,
            Subtotal = o.Subtotal,
            ShippingCost = o.ShippingCost,
            GrandTotal = o.GrandTotal,
            OrderStatusId = o.OrderStatusId,
            OrderStatusName = o.OrderStatus?.StatusName ?? "",
            OrderStatusColor = o.OrderStatus?.StatusColor ?? "",
            CreatedAt = o.CreatedAt,
            Items = o.OrderItems.OrderBy(i => i.Id).Select(i => new OrderItemDetailViewModel
            {
                Id = i.Id,
                PackageId = i.PackageId,
                PackageNameSnapshot = i.PackageNameSnapshot,
                UnitPriceSnapshot = i.UnitPriceSnapshot,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}
