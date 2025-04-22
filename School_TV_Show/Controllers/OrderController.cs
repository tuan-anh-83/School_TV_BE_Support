using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
using School_TV_Show.DTO;
using Services;
using System.Security.Claims;
using System.Text.Json;

namespace School_TV_Show.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IPackageService _packageService;
        private readonly PayOS _payOS;
        private readonly OrderTrackingService _orderTrackingService;
        public OrderController(
           IOrderService orderService,
           IOrderDetailService orderDetailService,
           IPackageService packageService,
           PayOS payOS)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _packageService = packageService;
            _payOS = payOS;
        }

        [HttpPost("create")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequestDTO request,
            [FromQuery] string returnUrl,
            [FromQuery] string cancelUrl)
        {
            try
            {
                Console.WriteLine($"Received Request: {JsonSerializer.Serialize(request)}");

                if (request == null || request.PackageID <= 0 || request.Quantity <= 0)
                {
                    return BadRequest(new { message = "PackageID and Quantity are required and must be greater than 0" });
                }

                var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (!int.TryParse(accountIdClaim, out int accountId))
                {
                    return Unauthorized("Invalid account information.");
                }

                var package = await _packageService.GetPackageByIdAsync(request.PackageID);
                if (package == null || package.Status != "Active")
                {
                    return BadRequest(new { message = "Invalid PackageID or package is not active." });
                }

                decimal totalPrice = package.Price * request.Quantity;

                var order = new Order
                {
                    AccountID = accountId,
                    OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), // Unique order code
                    Status = "Pending",
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);

                var orderDetail = new OrderDetail
                {
                    OrderID = createdOrder.OrderID,
                    PackageID = request.PackageID,
                    Quantity = request.Quantity,
                    Price = totalPrice
                };

                await _orderDetailService.CreateOrderDetailAsync(orderDetail);

                long uniquePaymentId = createdOrder.OrderCode; // Use OrderCode

                string description = $"{usernameClaim}-'{package.Name}'";

                List<ItemData> items = new()
        {
            new ItemData(package.Name, orderDetail.Quantity, (int)(package.Price))
        };

                PaymentData paymentData = new PaymentData(
                    uniquePaymentId,
                    (int)(orderDetail.Price),
                    description,
                    items,
                    cancelUrl,
                    returnUrl
                );

                CreatePaymentResult paymentResult = await _payOS.createPaymentLink(paymentData);
                Console.WriteLine($"PayOS Response: {JsonSerializer.Serialize(paymentResult)}");

                return Ok(new
                {
                    message = "Order created successfully",
                    orderId = createdOrder.OrderID,
                    paymentLink = paymentResult.checkoutUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "Error creating order", error = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving orders: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                if (orders == null || !orders.Any())
                    return NotFound("No orders found.");

                var activeOrders = orders.Where(o =>
                    o.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                    o.Status.Equals("Done", StringComparison.OrdinalIgnoreCase)
                );
                if (!activeOrders.Any())
                    return NotFound("No active orders found.");

                return Ok(activeOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving active orders: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found." });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> GetOrderHistory()
        {
            try
            {
                var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(accountIdClaim, out int accountId))
                {
                    return Unauthorized("Invalid account information.");
                }

                var orders = await _orderService.GetOrdersByAccountIdAsync(accountId);
                if (orders == null || !orders.Any())
                {
                    return NotFound(new { message = "No orders found for this account." });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order history: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest(new { message = "Status is required." });
            }

            try
            {
                var existingOrder = await _orderService.GetOrderByIdAsync(id);
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                existingOrder.Status = request.Status;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                await _orderService.UpdateOrderAsync(existingOrder);

                return Ok(new { message = "Order updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStatistics(
              [FromQuery] DateTime? startDate,
              [FromQuery] DateTime? endDate,
              [FromQuery] string interval = "daily")
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync(startDate, endDate, interval);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order statistics: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }

}
