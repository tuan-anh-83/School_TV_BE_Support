namespace School_TV_Show.DTO
{
    public class ChatMessageDTO
    {
        public int VideoHistoryID { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
    }
}
