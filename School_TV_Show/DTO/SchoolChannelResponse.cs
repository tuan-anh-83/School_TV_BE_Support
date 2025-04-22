namespace School_TV_Show.DTO
{
    public class SchoolChannelResponse
    {
        public int SchoolChannelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AccountID { get; set; }
        public string Website { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
