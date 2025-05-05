using Microsoft.AspNetCore.Mvc;

namespace School_TV_Show.DTO
{
    public class CreateAdScheduleRequestDTO 
    {
        public string Title { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string VideoUrl { get; set; }
    }
}
