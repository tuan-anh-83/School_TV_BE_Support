namespace School_TV_Show.DTO
{
    public class ChannelAnalyticsDTO
    {
        public int TotalViews { get; set; }
        public decimal ViewsComparisonPercent { get; set; }

        public double WatchTimeHours { get; set; }
        public decimal WatchTimeComparisonPercent { get; set; }

        public int NewFollowers { get; set; }
        public decimal FollowersComparisonPercent { get; set; }

        public string DateRange { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}
