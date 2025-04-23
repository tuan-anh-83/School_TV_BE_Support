using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class PaymentDAO
    {
        private static PaymentDAO instance = null;
        private readonly DataContext _context;
        private readonly PayOS _payOS;

        public PaymentDAO(DataContext dataContext, PayOS payOS)
        {
            _context = dataContext;
            _payOS = payOS;
        }

        public async Task<CreatePaymentResult> CreatePaymentAsync(Order order, string returnUrl, string cancelUrl)
        {
            List<ItemData> items = order.OrderDetails.Select(od =>
                new ItemData(od.Package.Name, (int)od.Quantity, (int)(od.Price))).ToList();

            PaymentData paymentData = new PaymentData(order.OrderID, (int)(order.TotalPrice),
                "Order Payment", items, cancelUrl, returnUrl);

            CreatePaymentResult paymentResult = await _payOS.createPaymentLink(paymentData);

            var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderID == order.OrderID);
            if (existingPayment == null)
            {
                var payment = new Payment
                {
                    OrderID = order.OrderID,
                    PaymentDate = DateTime.UtcNow,
                    Status = "Pending",
                    PaymentMethod = "Unknown",
                    Amount = order.TotalPrice
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            return paymentResult;
        }

        // ✅ Retrieve payment by order ID
        public async Task<Payment> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.OrderID == orderId);
        }

        // ✅ Update or insert payment record
        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderID == payment.OrderID);

            if (existingPayment == null)
            {
                _context.Payments.Add(payment);  // ✅ Insert if not exists
                Console.WriteLine($"✅ Payment inserted for Order {payment.OrderID}");
            }
            else
            {
                existingPayment.Status = payment.Status;
                existingPayment.PaymentMethod = payment.PaymentMethod;
                existingPayment.PaymentDate = payment.PaymentDate;
                existingPayment.Amount = payment.Amount;

                _context.Payments.Update(existingPayment);  // ✅ Update existing
                Console.WriteLine($"🔄 Payment updated for Order {payment.OrderID}");
            }

            await _context.SaveChangesAsync();
            return payment;
        }
    }
}
