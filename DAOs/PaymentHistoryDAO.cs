using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class PaymentHistoryDAO
    {
        private static PaymentHistoryDAO instance = null;
        private readonly DataContext _context;

        private PaymentHistoryDAO()
        {
            _context = new DataContext();
        }

        public static PaymentHistoryDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PaymentHistoryDAO();
                }
                return instance;
            }
        }

        public async Task AddPaymentHistoryAsync(Payment payment)
        {
            if (payment == null || payment.PaymentID <= 0)
            {
                Console.WriteLine("❌ Invalid Payment object. PaymentID is missing.");
                return;
            }

            var paymentHistory = new PaymentHistory
            {
                PaymentID = payment.PaymentID,
                Amount = payment.Amount,  // ✅ Assign amount
                Status = payment.Status,
                Timestamp = DateTime.UtcNow
            };

            await _context.PaymentHistories.AddAsync(paymentHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByPaymentIdAsync(int paymentId)
        {
            return await _context.PaymentHistories
                .Where(ph => ph.PaymentID == paymentId)
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }
        public async Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync()
        {
            return await _context.PaymentHistories
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId)
        {
            return await _context.PaymentHistories
                .Where(ph => ph.Payment.Order.AccountID == userId)
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }
    }
}
