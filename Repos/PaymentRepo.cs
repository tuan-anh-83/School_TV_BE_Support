using BOs.Models;
using DAOs;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class PaymentRepo : IPaymentRepo
    {
        private readonly PaymentDAO _paymentDAO;

        public PaymentRepo(PaymentDAO paymentDAO)
        {
            _paymentDAO = paymentDAO;
        }
        public async Task<CreatePaymentResult> CreatePaymentAsync(Order order, string returnUrl, string cancelUrl)
        {
            return await _paymentDAO.CreatePaymentAsync(order, returnUrl, cancelUrl);
        }

        public async Task<Payment> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _paymentDAO.GetPaymentByOrderIdAsync(orderId);
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            return await _paymentDAO.UpdatePaymentAsync(payment);
        }

    }
}
