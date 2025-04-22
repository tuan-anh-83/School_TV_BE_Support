namespace School_TV_Show.DTO
{
    public class CreateReplayScheduleRequestDTO
    {
        public int ProgramID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
