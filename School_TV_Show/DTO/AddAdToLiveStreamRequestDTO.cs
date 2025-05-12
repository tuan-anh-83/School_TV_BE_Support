namespace School_TV_Show.DTO
{
    public class AddAdToLiveStreamRequestDTO
    {
        public int ScheduleId { get; set; }
        public List<AdItem> Ads { get; set; }

        public class AdItem
        {
            public int AdScheduleId { get; set; }
            public string PlayAt { get; set; }
            public int Duration { get; set; }
        }
    }
}
