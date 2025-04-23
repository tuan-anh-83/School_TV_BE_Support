using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class OrderTrackingService
    {
        private readonly Dictionary<int, (string returnUrl, string cancelUrl)> _orderUrls = new();

        public void StoreOrderUrls(int orderId, string returnUrl, string cancelUrl)
        {
            _orderUrls[orderId] = (returnUrl, cancelUrl);
        }

        public (string returnUrl, string cancelUrl)? GetOrderUrls(int orderId)
        {
            return _orderUrls.TryGetValue(orderId, out var urls) ? urls : null;
        }
    }
}
