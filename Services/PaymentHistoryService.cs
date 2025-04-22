using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IPaymentHistoryRepo _repo;
        public PaymentHistoryService(IPaymentHistoryRepo repo)
        {
            _repo = repo;
        }

        public Task AddPaymentHistoryAsync(Payment payment)
        {
            return _repo.AddPaymentHistoryAsync(payment);
        }

        public async Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync()
        {
            return await _repo.GetAllPaymentHistoriesAsync();
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByPaymentIdAsync(int paymentId)
        {
            return await _repo.GetPaymentHistoriesByPaymentIdAsync(paymentId);
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId)
        {
            return await _repo.GetPaymentHistoriesByPaymentIdAsync(userId);
        }
    }
}
