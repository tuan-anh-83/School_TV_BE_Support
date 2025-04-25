namespace School_TV_Show.DTO
{
    public class CurrentPackageInfoDTO
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public int Duration { get; set; }
        public int TimeDuration { get; set; }
        public decimal Price { get; set; }
        public double? RemainingDuration { get; set; }
    }
}
