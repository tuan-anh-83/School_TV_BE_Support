using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Package
    {
        public int PackageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int TimeDuration { get; set; }
        public string Status { get; set; }
        public string ForType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //public ICollection<OrderDetail> OrderDetails { get; set; } 

        [InverseProperty("Package")]
        public virtual ICollection<AccountPackage> AccountPackages { get; set; } = new List<AccountPackage>();

    }
}
