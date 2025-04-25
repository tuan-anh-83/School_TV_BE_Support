using DAOs;
using Newtonsoft.Json;

namespace School_TV_Show.DTO
{
    public class CloudflareLiveStreamDTO
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public LiveStreamData Data { get; set; }
        public long Ts { get; set; }
    }

    public class LiveStreamData
    {
        [JsonProperty("notification_name")]
        public string NotificationName { get; set; }

        [JsonProperty("input_id")]
        public string InputId { get; set; }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
