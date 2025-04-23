using BOs.Models;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace School_TV_Show.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public PaymentController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] PayOSWebhookRequest request)
        {
            try
            {
                Console.WriteLine($"🔔 [WEBHOOK] Received Payment Webhook at {DateTime.UtcNow}");

                if (request == null || request.data == null)
                {
                    Console.WriteLine("❌ Webhook received NULL or invalid data!");
                    return BadRequest(new { success = false, message = "Invalid payload" });
                }

                Console.WriteLine($"🟢 Webhook Data: OrderCode = {request.data.orderCode}, Amount = {request.data.amount}, Transaction ID = {request.data.reference}");

                // ✅ Verify Signature
                bool isValidSignature = _paymentService.VerifySignature(request);
                Console.WriteLine($"🔑 Signature Valid: {isValidSignature}");
                if (!isValidSignature)
                {
                    Console.WriteLine($"❌ Webhook signature verification failed for Order {request.data.orderCode}!");
                    return BadRequest(new { success = false, message = "Invalid signature" });
                }

                // 🛑 Check if the order exists in the database
                var order = await _orderService.GetOrderByOrderCodeAsync(request.data.orderCode);

                if (order == null)
                {
                    Console.WriteLine($"⚠️ Order {request.data.orderCode} does not exist in the database. Ignoring webhook.");
                    return Ok(new { success = true, message = "Webhook received, but no action taken" });
                }

                Console.WriteLine($"✅ Order {order.OrderID} found. Proceeding with payment processing...");

                // ✅ Process Payment
                bool isUpdated = await _paymentService.HandlePaymentWebhookAsync(request);
                if (!isUpdated)
                {
                    Console.WriteLine($"⚠️ Order {order.OrderID} payment status unchanged.");
                    return Ok(new { success = true, message = "Order status unchanged" });
                }

                Console.WriteLine($"💰 Payment processed successfully for Order {order.OrderID}!");
                return Ok(new { success = true, message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ERROR] Webhook Processing Failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}

