namespace School_TV_Show.DTO
{
    public class BroadcastNotificationInput
    {
        public List<int> AccountIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? ProgramID { get; set; }
        public int? SchoolChannelID { get; set; }
    }
}
