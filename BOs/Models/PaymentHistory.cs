using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class PaymentHistory
    {
        public int PaymentHistoryID { get; set; }
        public int PaymentID { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }

        public Payment Payment { get; set; }
    }
}
