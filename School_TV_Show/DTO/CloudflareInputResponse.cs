namespace School_TV_Show.DTO
{
    public class CloudflareLiveInputResponse
    {
        public CloudflareLiveInput Result { get; set; }
        public bool Success { get; set; }
        public List<object> Errors { get; set; }
        public List<object> Messages { get; set; }
    }

    public class CloudflareLiveInput
    {
        public string Uid { get; set; }
        public CloudflareInputStatus Status { get; set; }
        public CloudflareRtmps Rtmps { get; set; }
        public CloudflareWebRTC WebRTCPlayback { get; set; }
        public CloudflareMeta Meta { get; set; }
    }

    public class CloudflareInputStatus
    {
        public CloudflareInputStatusCurrent Current { get; set; }
        public List<object> History { get; set; }
    }

    public class CloudflareInputStatusCurrent
    {
        public string IngestProtocol { get; set; }      // e.g. "rtmp"
        public string Reason { get; set; }              // e.g. "connected"
        public string State { get; set; }               // e.g. "connected", "disconnected"
        public DateTime StatusEnteredAt { get; set; }
        public DateTime StatusLastSeen { get; set; }
    }

    public class CloudflareRtmps
    {
        public string Url { get; set; }
        public string StreamKey { get; set; }
    }

    public class CloudflareWebRTC
    {
        public string Url { get; set; }
    }

    public class CloudflareMeta
    {
        public string Name { get; set; }
    }
}
