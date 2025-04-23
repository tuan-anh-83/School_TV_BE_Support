using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class News
    {
        public int NewsID { get; set; }
        public int SchoolChannelID { get; set; }
        public int CategoryNewsID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Status { get; set; }
        public bool FollowerMode { get; set; }
        public SchoolChannel SchoolChannel { get; set; }
        public CategoryNews CategoryNews { get; set; }
        public ICollection<NewsPicture> NewsPictures { get; set; }
    }
}
