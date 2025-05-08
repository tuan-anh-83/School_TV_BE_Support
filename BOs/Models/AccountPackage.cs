using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class AccountPackage
    {
        public int AccountPackageID { get; set; }
        public int AccountID { get; set; }
        public int PackageID { get; set; }

        public double TotalMinutesAllowed { get; set; }
        public double MinutesUsed { get; set; }
        public double RemainingMinutes { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? ExpiredAt { get; set; }

        [ForeignKey("PackageID")]
        [InverseProperty("AccountPackages")]
        public virtual Package? Package { get; set; } = null!;

        [ForeignKey("AccountID")]
        [InverseProperty("AccountPackages")]
        public virtual Account? Account { get; set; } = null!;
    }
}
