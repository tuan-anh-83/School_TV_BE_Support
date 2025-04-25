using Microsoft.AspNetCore.Mvc;

namespace School_TV_Show.DTO
{
    public class PackageAdminResponse
    {
        public int PackageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int TimeDuration { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
