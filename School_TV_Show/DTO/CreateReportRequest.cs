namespace School_TV_Show.DTO
{
    public class CreateReportRequest
    {
        public int AccountID { get; set; }
        public int VideoHistoryID { get; set; }
        public string Reason { get; set; }
    }
}
