using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasketApi
{
    public static class BasketSettings
    {
        public static int Id { get; set; }
        public static double DeliveryFee { get; set; }
        public static string Currency { get; set; }
        public static string BannerImageUrl { get; set; }
        public static double FreeDeliveryThreshold { get; set; }
        public static string HowItWorksUrl { get; set; }
        public static string HowItWorksDescription { get; set; }
        public static string InstagramImage { get; set; }

        public static void LoadSettings()
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    var setting = ctx.Settings.FirstOrDefault();
                    if (setting != null)
                    {
                        Id = setting.Id;
                        DeliveryFee = setting.DeliveryFee;
                        Currency = setting.Currency;
                        BannerImageUrl = setting.BannerImage;
                        FreeDeliveryThreshold = setting.FreeDeliveryThreshold;
                        HowItWorksUrl = setting.HowItWorksUrl;
                        HowItWorksDescription = setting.HowItWorksDescription;
                        InstagramImage = setting.InstagramImage;
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }
    }
}