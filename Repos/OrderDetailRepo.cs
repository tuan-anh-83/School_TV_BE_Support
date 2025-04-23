using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class OrderDetailRepo : IOrderDetailRepo
    {
        public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            return await OrderDetailDAO.Instance.CreateOrderDetailAsync(orderDetail);   
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            return await OrderDetailDAO.Instance.GetOrderDetailsByOrderIdAsync(orderId);
        }
    }
}
