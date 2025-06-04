using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> AddPaymentHistoryAsync(Payment payment)
        {
            try
            {
                if (payment == null || payment.PaymentID <= 0)
                {
                    Console.WriteLine("❌ Invalid Payment object. PaymentID is missing.");
                    return false;
                }

                var paymentHistory = new PaymentHistory
                {
                    PaymentID = payment.PaymentID,
                    Amount = payment.Amount,  // ✅ Assign amount
                    Status = payment.Status,
                    Timestamp = DateTime.UtcNow
                };

                await _context.PaymentHistories.AddAsync(paymentHistory);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByPaymentIdAsync(int paymentId)
        {
            return await _context.PaymentHistories.AsNoTracking()
                .Where(ph => ph.PaymentID == paymentId)
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }
        public async Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync()
        {
            return await _context.PaymentHistories
                .AsNoTracking()
                .Include(ph => ph.Payment)
                    .ThenInclude(p => p.Order)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Package)
                .Include(ph => ph.Payment)
                    .ThenInclude(p => p.Order)
                        .ThenInclude(o => o.Account)
                            .ThenInclude(a => a.Role)
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }

        public async Task<List<PaymentHistory>> GetPaymentHistoriesByUserIdAsync(int userId)
        {
            return await _context.PaymentHistories.AsNoTracking()
             .Where(ph => ph.Payment.Order.AccountID == userId)
                .OrderByDescending(ph => ph.Timestamp)
                .ToListAsync();
        }
    }
}
