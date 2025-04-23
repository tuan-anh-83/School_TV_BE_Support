using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class OrderRepo : IOrderRepo
    {
        public async Task<Order> CreateOrderAsync(Order order)
        {
            return await OrderDAO.Instance.CreateOrderAsync(order);
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            return await OrderDAO.Instance.DeleteOrderAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await OrderDAO.Instance.GetAllOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await OrderDAO.Instance.GetOrderByIdAsync(orderId);
        }

        public async Task<Order> GetOrderByOrderCodeAsync(long orderCode)
        {
            return await OrderDAO.Instance.GetOrderByOrderCodeAsync(orderCode);    
        }

        public async Task<IEnumerable<Order>> GetOrdersByAccountIdAsync(int accountId)
        {
            return await OrderDAO.Instance.GetOrdersByAccountIdAsync(accountId);
        }

        public async Task<object> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate, string interval)
        {
            return await OrderDAO.Instance.GetOrderStatisticsAsync(startDate, endDate, interval);
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersOlderThanAsync(TimeSpan timeSpan)
        {
            return await OrderDAO.Instance.GetPendingOrdersOlderThanAsync(timeSpan);
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            return await OrderDAO.Instance.UpdateOrderAsync(order);
        }
    }
}
