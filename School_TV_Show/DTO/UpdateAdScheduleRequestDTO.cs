namespace School_TV_Show.DTO
{
    public class UpdateAdScheduleRequestDTO
    {
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string VideoUrl { get; set; }
    }
}
