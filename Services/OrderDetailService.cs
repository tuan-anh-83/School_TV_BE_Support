using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepo _orderDetailRepo;

        public OrderDetailService(IOrderDetailRepo orderDetailRepo)
        {
            _orderDetailRepo = orderDetailRepo;
        }
        public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            return await _orderDetailRepo.CreateOrderDetailAsync(orderDetail);
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            return await _orderDetailRepo.GetOrderDetailsByOrderIdAsync(orderId);
        }
    }
}
