namespace School_TV_Show.DTO
{
    public class ProgramResponse
    {
        public int ProgramID { get; set; }
        public string ProgramName { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int SchoolChannelID { get; set; }
        public SchoolChannelResponse SchoolChannel { get; set; }

        public List<ScheduleResponse> Schedules { get; set; }
    }
}
