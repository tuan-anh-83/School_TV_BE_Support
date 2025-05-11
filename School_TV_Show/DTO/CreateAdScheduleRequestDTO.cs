using Microsoft.AspNetCore.Mvc;

namespace School_TV_Show.DTO
{
    public class CreateAdScheduleRequestDTO 
    {
        public string Title { get; set; }
        public int DurationSeconds { get; set; }
        public IFormFile VideoFile { get; set; }
    }
}
