namespace School_TV_Show.DTO
{
    public class UpdateAdScheduleRequestDTO
    {
        public string Title { get; set; }
        public int DurationSeconds { get; set; }
        public IFormFile? VideoUrl { get; set; }
    }
}
