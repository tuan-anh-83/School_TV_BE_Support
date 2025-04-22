using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IPaymentHistoryService
    {
        Task AddPaymentHistoryAsync(Payment payment);
        Task<List<PaymentHistory>> GetPaymentHistoriesByPaymentIdAsync(int paymentId);

        Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync();
        Task<List<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId);
    }
}
