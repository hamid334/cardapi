using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Settings
    {
        [JsonIgnore]
        public int Id { get; set; }
        public double DeliveryFee { get; set; }
        public string Currency { get; set; }
        public string HowItWorksUrl { get; set; }
        public string HowItWorksDescription { get; set; }
        public string AboutUs { get; set; }
        public string BannerImage { get; set; } 
        public double FreeDeliveryThreshold { get; set; }
        public string InstagramImage { get; set; }
    }
}
