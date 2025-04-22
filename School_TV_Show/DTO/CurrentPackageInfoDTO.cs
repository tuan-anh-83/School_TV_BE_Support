namespace School_TV_Show.DTO
{
    public class CurrentPackageInfoDTO
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int? RemainingDuration { get; set; }
    }
}
