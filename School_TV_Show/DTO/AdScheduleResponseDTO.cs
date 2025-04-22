using Microsoft.AspNetCore.Mvc;

namespace School_TV_Show.DTO
{
    public class AdScheduleResponseDTO
    {
        public int AdScheduleID { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
