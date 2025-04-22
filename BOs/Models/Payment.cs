using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        
        public string PaymentMethod { get; set; } = "Unknown"; 
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; 
        public DateTime PaymentDate { get; set; }

        public Order Order { get; set; }
        public ICollection<PaymentHistory> PaymentHistories { get; set; }
    }
}
