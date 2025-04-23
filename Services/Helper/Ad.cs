using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helper
{
    /*  public static class MockAdvertisements
    {
        public static List<Advertisement> GetMockAdvertisements()
        {
            return new List<Advertisement>
        {
            new Advertisement { AdID = 1, AdName = "Coca-Cola Ad", Duration = TimeSpan.FromMinutes(5), AdLink = "https://example.com/coca_cola_ad" },
            new Advertisement { AdID = 2, AdName = "Nike Ad", Duration = TimeSpan.FromMinutes(3), AdLink = "https://example.com/nike_ad" },
            new Advertisement { AdID = 3, AdName = "Apple Ad", Duration = TimeSpan.FromMinutes(4), AdLink = "https://example.com/apple_ad" },
            new Advertisement { AdID = 4, AdName = "Samsung Ad", Duration = TimeSpan.FromMinutes(2), AdLink = "https://example.com/samsung_ad" },
            new Advertisement { AdID = 5, AdName = "Honda Ad", Duration = TimeSpan.FromMinutes(6), AdLink = "https://example.com/honda_ad" }
        };
        }
    }*/

    // Định nghĩa class cho quảng cáo
    public class Advertisement
    {
        public int AdID { get; set; }
        public string AdName { get; set; }
        public TimeSpan Duration { get; set; }
        public string AdURL { get; set; }
    }
}
