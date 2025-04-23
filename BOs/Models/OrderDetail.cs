using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int PackageID { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Order Order { get; set; }

        public Package Package { get; set; }
    }
}
