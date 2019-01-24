using BasketApi.DomainModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using static BasketApi.Global;

namespace BasketApi
{
    public static class Utility
    {
        private static HttpClient client = new HttpClient();

        public static string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];

        public static IEnumerable<T> Page<T>(this IEnumerable<T> en, int pageSize, int page)
        {
            return en.Skip(page * pageSize).Take(pageSize);
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> en, int pageSize, int page)
        {
            return en.Skip(page * pageSize).Take(pageSize);
        }

        public static async Task GenerateToken(this User user, HttpRequestMessage request)
        {
            try
            {
                var parameters = new Dictionary<string, string>{
                            { "username", user.Email },
                            { "password", user.Password },
                            { "grant_type", "password" },
                            { "signintype", "0" }
                        };

                var content = new FormUrlEncodedContent(parameters);
                var baseUrl = request.RequestUri.AbsoluteUri.Substring(0, request.RequestUri.AbsoluteUri.IndexOf("api"));
                var response = await client.PostAsync(baseUrl + "token", content);

                user.Token = await response.Content.ReadAsAsync<Token>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task GenerateToken(this DeliveryMan user, HttpRequestMessage request)
        {
            try
            {
                var parameters = new Dictionary<string, string>{
                            { "username", user.Email },
                            { "password", user.Password },
                            { "grant_type", "password" },
                            { "signintype", "1" }
                        };

                var content = new FormUrlEncodedContent(parameters);
                var baseUrl = request.RequestUri.AbsoluteUri.Substring(0, request.RequestUri.AbsoluteUri.IndexOf("api"));
                var response = await client.PostAsync(baseUrl + "token", content);

                user.Token = await response.Content.ReadAsAsync<Token>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task GenerateToken(this Admin user, HttpRequestMessage request)
        {
            try
            {
                var parameters = new Dictionary<string, string>{
                            { "username", user.Email },
                            { "password", user.Password },
                            { "grant_type", "password" },
                            { "signintype", user.Role.ToString() }
                        };

                var content = new FormUrlEncodedContent(parameters);
                var baseUrl = request.RequestUri.AbsoluteUri.Substring(0, request.RequestUri.AbsoluteUri.IndexOf("api"));
                var response = await client.PostAsync(baseUrl + "token", content);

                user.Token = await response.Content.ReadAsAsync<Token>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static HttpStatusCode LogError(Exception ex)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "/ErrorLog.txt"))
                {
                    sw.WriteLine("DateTime : " + DateTime.Now + Environment.NewLine);
                    if (ex.Message != null)
                    {
                        sw.WriteLine(Environment.NewLine + "Message" + ex.Message);
                        sw.WriteLine(Environment.NewLine + "StackTrace" + ex.StackTrace);
                    }
                    again: if (ex.InnerException != null)
                    {
                        sw.WriteLine(Environment.NewLine + "Inner Exception : " + ex.InnerException.Message);
                        //if (ex.InnerException.InnerException != null)
                        //{
                        //    sw.WriteLine(Environment.NewLine + "Inner Exception : " + ex.InnerException.Message);
                        //}
                    }
                    if (ex.InnerException.InnerException != null)
                    {
                        ex = ex.InnerException;
                        goto again;
                    }

                    sw.WriteLine("------******------");
                }
                return HttpStatusCode.InternalServerError;
            }
            catch (Exception)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public static HttpStatusCode LogErrorString(string str)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "/ErrorLog.txt"))
                {
                    sw.WriteLine("DateTime : " + DateTime.Now + Environment.NewLine);
                    sw.WriteLine(Environment.NewLine + "Message" + str);

                    sw.WriteLine("------******------");
                }
                return HttpStatusCode.InternalServerError;
            }
            catch (Exception)
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public static string GenerateMerchantPin()
        {
            const string AllowedChars =
       "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random rng = new Random();

            return RandomStrings(AllowedChars, 4, 4, 25, rng).FirstOrDefault();
        }
        private static IEnumerable<string> RandomStrings(
    string allowedChars,
    int minLength,
    int maxLength,
    int count,
    Random rng)
        {
            char[] chars = new char[maxLength];
            int setLength = allowedChars.Length;

            while (count-- > 0)
            {
                int length = rng.Next(minLength, maxLength + 1);

                for (int i = 0; i < length; ++i)
                {
                    chars[i] = allowedChars[rng.Next(setLength)];
                }

                yield return new string(chars, 0, length);
            }
        }

        public static DbGeography CreatePoint(double lat, double lon, int srid = 4326)
        {
            string wkt = String.Format("POINT({0} {1})", lon, lat);

            return DbGeography.PointFromText(wkt, srid);
        }

        public enum SubscriptionStatus
        {
            InActive,
            Active
        }

        public enum BasketEntityTypes
        {
            Product,
            Category,
            Store,
            Package,
            Admin,
            Offer,
            Box,
            City = 8
        }

        public static string GetOrderStatusName(int orderStatus)
        {
            try
            {
                switch (orderStatus)
                {
                    case (int)OrderStatuses.AssignedToDeliverer:
                        return "Assigned To Deliverer";
                    case (int)OrderStatuses.DelivererInProgress:
                        return "Deliverer In Progress";
                    case (int)OrderStatuses.InProgress:
                        return "In Progress";
                    case (int)OrderStatuses.ReadyForDelivery:
                        return "Ready For Delivery";
                    default:
                        return ((OrderStatuses)orderStatus).ToString();
                }

            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                return null;
            }
        }


        public static string GetBoxCategoryName(int CategoryId)
        {
            try
            {
                switch (CategoryId)
                {
                    case (int)BoxCategoryOptions.Junior:
                        return "Junior";
                    case (int)BoxCategoryOptions.Monthly:
                        return "Monthly";
                    case (int)BoxCategoryOptions.ProBox:
                        return "TriMonthly";
                    case (int)BoxCategoryOptions.HallOfFame:
                        return "Hall Of Fame";
                    default:
                        return "Invalid";
                }

            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                return null;
            }
        }


        public static void DeleteFileIfExists(string path)
        {
            try
            {
                var filePath = HttpContext.Current.Server.MapPath("~/" + path);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public static void SendPushNotifications(List<UserDevice> usersToPushAndroid, List<UserDevice> usersToPushIOS, Notification Notification, int PushType)
        {
            try
            {

                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                {
                    Global.objPushNotifications.SendAndroidPushNotification
                    (
                        usersToPushAndroid,
                        OtherNotification: Notification,
                        Type: PushType);

                    //    //    Global.objPushNotifications.SendIOSPushNotification
                    //    //    (
                    //    //        usersToPushIOS,
                    //    //        OtherNotification: Notification,
                    //    //        Type: PushType);
                    //    //
                });

            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }
    }



    public class EmailUtil
    {
        public static string FromEmail = ConfigurationManager.AppSettings["FromMailAddress"];
        public static string FromName = ConfigurationManager.AppSettings["FromMailName"];
        public static string FromPassword = ConfigurationManager.AppSettings["FromPassword"];
        public static MailAddress FromMailAddress = new MailAddress(FromEmail, FromName);
    }
}