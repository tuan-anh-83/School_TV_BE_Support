using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ClouflareDTO
{
    public class CloudflareMetadataResponse
    {
        public CloudflareMetadata Result { get; set; }
    }

    public class CloudflareMetadata
    {
        public double Duration { get; set; }
        public string Uid { get; set; }
        public CloudflareStatus Status { get; set; }  // ✅ sửa ở đây
    }

    public class CloudflareStatus
    {
        public string State { get; set; }
        public string PctComplete { get; set; }
        public string ErrorReasonCode { get; set; }
        public string ErrorReasonText { get; set; }
    }

    public class CloudflareDownloadStatusResponse
    {
        public CloudflareDownloadResult Result { get; set; }
    }

    public class CloudflareDownloadResult
    {
        public CloudflareDownloadInfo Default { get; set; }
    }

    public class CloudflareDownloadInfo
    {
        public string Status { get; set; }
        public string Url { get; set; }
    }

    public class CloudflareVideoListResponse
    {
        public List<CloudflareVideo> Result { get; set; }
    }

    public class CloudflareVideo
    {
        public string Uid { get; set; }
    }
}
