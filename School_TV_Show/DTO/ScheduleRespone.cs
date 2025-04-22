namespace School_TV_Show.DTO
{
    public class ScheduleResponse
    {
        public int ScheduleID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public bool LiveStreamStarted { get; set; }
        public bool LiveStreamEnded { get; set; }
        public int ProgramID { get; set; }
    }
}
