using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class PaymentHistoryRepo : IPaymentHistoryRepo
    {
        public Task AddPaymentHistoryAsync(Payment payment)
        {
            return PaymentHistoryDAO.Instance.AddPaymentHistoryAsync(payment);  
        }

        public async Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync()
        {
            return await PaymentHistoryDAO.Instance.GetAllPaymentHistoriesAsync();  
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByPaymentIdAsync(int paymentId)
        {
            return await PaymentHistoryDAO.Instance.GetPaymentHistoriesByPaymentIdAsync(paymentId);
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId)
        {
            return await PaymentHistoryDAO.Instance.GetPaymentHistoriesByUserIdAsync(userId);
        }
    }
}
