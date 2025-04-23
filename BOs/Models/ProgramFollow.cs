using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class ProgramFollow
    {
        public int ProgramFollowID { get; set; }
        public int AccountID { get; set; }
        public int ProgramID { get; set; }
        public string Status { get; set; }
        public DateTime FollowedAt { get; set; }

        [JsonIgnore]
        public Account Account { get; set; }

        [JsonIgnore]
        public Program Program { get; set; }
    }
}
