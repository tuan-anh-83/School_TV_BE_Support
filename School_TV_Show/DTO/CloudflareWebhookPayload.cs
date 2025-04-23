namespace School_TV_Show.DTO
{
    public class CloudflareWebhookPayload
    {
        public ResultData Result { get; set; }

        public class ResultData
        {
            public string Uid { get; set; }
            public StatusData Status { get; set; }
        }

        public class StatusData
        {
            public CurrentStatus Current { get; set; }
        }

        public class CurrentStatus
        {
            public string State { get; set; }  // "disconnected"
        }
    }
}
