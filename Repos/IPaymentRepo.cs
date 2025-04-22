using BOs.Models;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IPaymentRepo
    {
        Task<CreatePaymentResult> CreatePaymentAsync(Order order, string returnUrl, string cancelUrl);
        Task<Payment> GetPaymentByOrderIdAsync(int orderId);
        Task<Payment> UpdatePaymentAsync(Payment payment);
    }
}
