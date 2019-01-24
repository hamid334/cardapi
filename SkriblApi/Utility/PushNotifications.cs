using BasketApi;
using BasketApi.Models;
using DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static BasketApi.Global;

namespace BasketApi
{
    /// <summary>
    /// This class has configuration settings for Enterprise Android and IOS Apps
    /// </summary>
    public class Enterprise
    {
        /// <summary>
        /// IOS Configuration for Enterprise Apps
        /// </summary>
        public class IOS
        {
            /// <summary>
            /// Sandbox Certificate Name(used for IOS)
            /// </summary>
            public static string APNSDistributionCertificateName { get; set; }
            /// <summary>
            /// Production Certificate Name(used for IOS)
            /// </summary>
            public static string APNSDevelopmentCertificateName { get; set; }
            /// <summary>
            /// Production Configuration(IOS)
            /// </summary>
            public static ApnsConfiguration ProductionConfig { get; set; }
            /// <summary>
            /// Sandbox Configuration(IOS)
            /// </summary>
            public static ApnsConfiguration SandboxConfig { get; set; }
        }

        /// <summary>
        /// Android Configuration
        /// </summary>
        public class Android
        {
            public static GcmConfiguration AndroidGCMConfig { get; set; }
            public static string PackageName { get; set; }
        }
    }

    /// <summary>
    /// This class has configuration settings for Playstore Android and IOS Apps
    /// </summary>
    public class PlayStore
    {
        public class IOS
        {
            public static string APNSDistributionCertificateName { get; set; }
            public static string APNSDevelopmentCertificateName { get; set; }
            public static ApnsConfiguration ProductionConfig { get; set; }
            public static ApnsConfiguration SandboxConfig { get; set; }
        }
        public class Android
        {
            public static GcmConfiguration AndroidGCMConfig { get; set; }
            public static string PackageName { get; set; }

        }

    }

    public class PushNotifications
    {
        #region Fields Declaration

        public EventHandler DeviceRemoved;
        private ApnsConfiguration ApnsConfig;
        private GcmConfiguration FCMConfig;
        private string GCMAppStorePackageName;
        private string GCMEnterprisePackageName;
        private string GCMProjectID;
        private string GCMWebAPIKey;
        private string GCMURL;
        public string APNSFilePasswordKey;
        //private string APNSDistributionCertificateName;
        //private string APNSDevelopmentCertificateName;


        #endregion

        void initializeConfiguration()
        {
            try
            {
                #region GCMConfiguration

                GCMProjectID = ConfigurationManager.AppSettings["GCMProjectID"];
                GCMWebAPIKey = ConfigurationManager.AppSettings["GCMWebAPIKey"];
                GCMURL = ConfigurationManager.AppSettings["GCMURL"];

                #region AndroidEnterprise

                Enterprise.Android.PackageName = ConfigurationManager.AppSettings["GCMEnterprisePackageName"];
                Enterprise.Android.AndroidGCMConfig = new GcmConfiguration(GCMProjectID, GCMWebAPIKey, Enterprise.Android.PackageName);
                Enterprise.Android.AndroidGCMConfig.GcmUrl = GCMURL;

                #endregion

                #region AndroidPlayStore

                PlayStore.Android.PackageName = ConfigurationManager.AppSettings["GCMAppStorePackageName"];
                PlayStore.Android.AndroidGCMConfig = new GcmConfiguration(GCMProjectID, GCMWebAPIKey, PlayStore.Android.PackageName);
                PlayStore.Android.AndroidGCMConfig.GcmUrl = GCMURL;

                #endregion

                #endregion

                #region ApnsConfiguration

                APNSFilePasswordKey = ConfigurationManager.AppSettings["APNSCertPassword"];

                #region IOSEnterprise
                Enterprise.IOS.APNSDevelopmentCertificateName = ConfigurationManager.AppSettings["APNSEnterpriseDevelopmentCertificateName"];
                Enterprise.IOS.APNSDistributionCertificateName = ConfigurationManager.AppSettings["APNSEnterpriseDistributionCertificateName"];

                //Enterprise.IOS.ProductionConfig = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production,
                //                AppDomain.CurrentDomain.BaseDirectory + Enterprise.IOS.APNSDistributionCertificateName,
                //                APNSFilePasswordKey);

                Enterprise.IOS.SandboxConfig = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                                AppDomain.CurrentDomain.BaseDirectory + Enterprise.IOS.APNSDevelopmentCertificateName,
                                APNSFilePasswordKey);
                #endregion

                #region IOSPlayStore

                PlayStore.IOS.APNSDevelopmentCertificateName = ConfigurationManager.AppSettings["APNSStoreDevelopmentCertificateName"];
                PlayStore.IOS.APNSDistributionCertificateName = ConfigurationManager.AppSettings["APNSStoreDistributionCertificateName"];

                //PlayStore.IOS.ProductionConfig = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production,
                //                AppDomain.CurrentDomain.BaseDirectory + PlayStore.IOS.APNSDistributionCertificateName,
                //                APNSFilePasswordKey);

                PlayStore.IOS.SandboxConfig = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                                AppDomain.CurrentDomain.BaseDirectory + PlayStore.IOS.APNSDevelopmentCertificateName,
                                APNSFilePasswordKey);

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public enum FileType
        {
            Photo = 1,
            Video = 2,
            Audio = 3,
            Excel = 4,
            Pdf = 5,
            Doc = 6,
            Pppt = 7,
            Txt = 8,
            Other = 9


        };

        public PushNotifications(bool ProductionEnvironment)
        {
            try
            {
                //Enterprise.IOS.APNSDistributionCertificateName = ConfigurationManager.AppSettings["APNSEnterpriseDistributionCertificateName"];
                //Enterprise.IOS.APNSDevelopmentCertificateName = ConfigurationManager.AppSettings["APNSEnterpriseDistributionCertificateName"];

                initializeConfiguration();

                FCMConfig = new GcmConfiguration(GCMProjectID, GCMWebAPIKey, Enterprise.Android.PackageName);
                FCMConfig.GcmUrl = GCMURL;

                //By default, Enterprise settings.
                ApnsConfig = new ApnsConfiguration(
                    (ProductionEnvironment) ? ApnsConfiguration.ApnsServerEnvironment.Production : ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                       (ProductionEnvironment) ?
                       AppDomain.CurrentDomain.BaseDirectory + Enterprise.IOS.APNSDistributionCertificateName :
                       AppDomain.CurrentDomain.BaseDirectory + Enterprise.IOS.APNSDevelopmentCertificateName,
                       APNSFilePasswordKey);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }


        public class IosPush
        {
            public IosPush()
            {
                aps = new aps();
                notification = new NotificationModel();
            }

            public aps aps { get; set; }
            public NotificationModel notification { get; set; }
        }

        public class aps
        {
            public aps()
            {
                alert = new alert();
            }
            public alert alert { get; set; }
            public string sound { get; set; } = "Default";
            public int badge { get; set; } = 1;
            public int contentavailable { get; set; }
        }

        public class alert
        {
            public string title { get; set; }
            public string body { get; set; }
        }

        public class NotificationModel
        {
            public string Title { get; set; }

            public string Message { get; set; }

            public int NotificationId { get; set; }

            public int EntityType { get; set; }

            public int EntityId { get; set; }
        }


        // from here
        public class NotificationMessage
        {
            public string[] registration_ids { get; set; }
            public SendNotification notification { get; set; }
            public object data { get; set; }
        }
        public class SendNotification
        {
            public string title { get; set; }
            public string text { get; set; }
        }
        public class DynamicValuesModel
        {
            public int entityid { get; set; }
            public int entitytype { get; set; }
            public int notificationid { get; set; }
            public bool isread { get; set; }
        }

        public void SendIOSPushNotification(List<UserDevice> devices, AdminNotifications AdminNotification = null, Notification OtherNotification = null, int EntityType = 0, int EntityId = 0)
        {
            string serverKey = GCMWebAPIKey;
            var result = "-1";
            var notificationid = 0;

            try
            {
                var webAddr = "https://fcm.googleapis.com/fcm/send";

                foreach (var device in devices.Where(x => x.IsActive))
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "key=" + serverKey);
                    httpWebRequest.Method = "POST";
                    IosPush pushModel = new IosPush();
                    if (AdminNotification != null)
                    {
                        Utility.LogErrorString("Notification:Not Null");
                        pushModel.aps.alert.title = AdminNotification.Title;
                        pushModel.aps.alert.body = AdminNotification.Description;
                        //using (RiscoContext ctx=new RiscoContext())
                        //{
                        //    ctx.Notifications.Count();
                        //}

                        //if (device.User != null)
                        //    pushModel.notification.NotificationId = device.User.SendingUserNotifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id).Id;
                        //else
                        //pushModel.notification.NotificationId = device.DeliveryMan.Notifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id).Id;
                        pushModel.notification.EntityType = (int)PushNotificationType.Announcement;
                        //pushModel.notification.EntityId = OtherNotification.Entity_ID.Value;
                        pushModel.aps.contentavailable = 1;
                    }
                    else
                    {
                        Utility.LogErrorString("Notification: Null");
                        pushModel.aps.alert.title = OtherNotification.Title;
                        pushModel.aps.alert.body = OtherNotification.Text;
                        pushModel.notification.NotificationId = OtherNotification.Id;
                        //pushModel.notification.DeliveryMan_Id = OtherNotification.DeliveryMan_ID;
                        pushModel.notification.EntityType = EntityType;
                        pushModel.notification.EntityId = 1;
                        pushModel.aps.contentavailable = 1;
                    }

                    using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        var dta = new DynamicValuesModel
                        {
                            entityid = pushModel.notification.EntityId,
                            entitytype = pushModel.notification.EntityType,
                            notificationid = pushModel.notification.NotificationId,
                            isread = true
                        };



                        var messageInformation = new NotificationMessage();
                        //string[] authTokens = new string[1];
                        //authTokens[0]=new string
                        string[] authTokens = { device.AuthToken };

                        messageInformation.notification = new SendNotification()
                        {
                            title = pushModel.aps.alert.title,
                            text = pushModel.aps.alert.body
                        };
                        messageInformation.registration_ids = authTokens;

                        messageInformation.data = dta;
                        //Object to JSON STRUCTURE => using Newtonsoft.Json;
                        string jsonMessage = JsonConvert.SerializeObject(messageInformation);


                        //string json = "";
                        //json = "{\"to\": \"" + device.AuthToken + "\",\"notification\": {\"body\": \"" + pushModel.aps.alert.body + "\",\"title\": \"" + pushModel.aps.alert.title + "\",\"notificationid\": \"" + pushModel.notification.NotificationId + "\",\"entitytype\": \"" + pushModel.notification.EntityType + "\",\"entityid\": \"" + pushModel.notification.EntityId + "\",\"isread\": \"" + true + "\",}}";
                        //json = "{\"to\": \"" + device.AuthToken + "\",\"notification\": {\"text\": \"" + pushModel.aps.alert.body + "\",\"title\": \"" + pushModel.aps.alert.title + "\",\"notificationid\": \"" + notificationid + "\",\"isread\": \"" + true + "\",},\"data\":{\"entitytype\": \"" + pushModel.notification.EntityType + "\",\"entityid\": \"" + pushModel.notification.EntityId + "\",}}";
                        streamWriter.Write(jsonMessage);
                        streamWriter.Flush();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                        if (result.Contains("success"))
                        {
                            Utility.LogErrorString("Notification:sending");
                            dynamic token = JObject.Parse(result);
                            string success = token.success.ToString();
                            //return success == "1" ? true : false;
                        }
                        else
                        {
                            Utility.LogErrorString("Notification:sending Fail");
                        }
                    }
                }

                // return result;
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }

        }

        //public void SendIOSPushNotification(List<UserDevice> devices, AdminNotifications AdminNotification = null, Notification OtherNotification = null, int EntityType = 0, int EntityId = 0)
        //{
        //    string serverKey = GCMWebAPIKey;
        //    var result = "-1";
        //    var notificationid = 0;

        //    try
        //    {
        //        var webAddr = "https://fcm.googleapis.com/fcm/send";

        //        foreach (var device in devices.Where(x => x.IsActive))
        //        {
        //            var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
        //            httpWebRequest.ContentType = "application/json";
        //            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "key=" + serverKey);
        //            httpWebRequest.Method = "POST";
        //            NotificationMessage pushModel = new NotificationMessage();
        //            if (AdminNotification != null)
        //            {

        //                pushModel.aps.alert.title = AdminNotification.Title;
        //                pushModel.aps.alert.body = AdminNotification.Description;
        //                if (device.User != null)
        //                    pushModel.notification.NotificationId = device.User.SendingUserNotifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id).Id;
        //                else
        //                    //pushModel.notification.NotificationId = device.DeliveryMan.Notifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id).Id;
        //                    pushModel.notification.EntityType = (int)PushNotificationType.Announcement;
        //                //pushModel.notification.EntityId = OtherNotification.Entity_ID.Value;
        //                pushModel.aps.contentavailable = 1;
        //            }
        //            else
        //            {
        //                pushModel.aps.alert.title = OtherNotification.Title;
        //                pushModel.aps.alert.body = OtherNotification.Description;
        //                pushModel.notification.NotificationId = OtherNotification.Id;
        //                //pushModel.notification.DeliveryMan_Id = OtherNotification.DeliveryMan_ID;
        //                pushModel.notification.EntityType = EntityType;
        //                pushModel.notification.EntityId = OtherNotification.EntityId;
        //                pushModel.aps.contentavailable = 1;
        //            }

        //            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        //            {
        //                var dta = new DynamicValuesModel
        //                {
        //                    entityid = pushModel.notification.EntityId,
        //                    entitytype = pushModel.notification.EntityType,
        //                    notificationid = pushModel.notification.NotificationId,
        //                    isread = true
        //                };



        //                var messageInformation = new NotificationMessage();
        //                //string[] authTokens = new string[1];
        //                //authTokens[0]=new string
        //                string[] authTokens = { device.AuthToken };

        //                messageInformation.notification = new SendNotification()
        //                {
        //                    title = pushModel.aps.alert.title,
        //                    text = pushModel.aps.alert.body
        //                };
        //                messageInformation.registration_ids = authTokens;

        //                messageInformation.data = dta;
        //                //Object to JSON STRUCTURE => using Newtonsoft.Json;
        //                string jsonMessage = JsonConvert.SerializeObject(messageInformation);
        //                streamWriter.Write(jsonMessage);
        //                streamWriter.Flush();
        //            }

        //            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                result = streamReader.ReadToEnd();
        //                if (result.Contains("success") && result.Contains("failure"))
        //                {
        //                    dynamic token = JObject.Parse(result);
        //                    string success = token.success.ToString();
        //                    //return success == "1" ? true : false;
        //                }
        //                else
        //                {
        //                }
        //            }
        //        }

        //        // return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        //    }

        //        //}



        //public void SendIOSPushNotification(List<UserDevice> devices, AdminNotifications AdminNotification = null, Notification OtherNotification = null, int Type = 0)
        //{
        //    try
        //    {
        //        // Configuration (NOTE: .pfx can also be used here)
        //        if (devices.Count() == 0) //it means there is no device no need to run the below code
        //        {
        //            return;
        //        }

        //        if (ApnsConfig != null)
        //        {
        //            IosPush pushModel = new IosPush();

        //            foreach (var device in devices.Where(x => x.IsActive))
        //            {
        //                if (AdminNotification != null)
        //                {
        //                    pushModel.aps.alert.title = AdminNotification.Title;
        //                    pushModel.aps.alert.body = AdminNotification.Description;
        //                    pushModel.notification.NotificationId = device.User.Notifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id).Id;
        //                    pushModel.notification.Type = (int)PushNotificationType.Announcement;
        //                }
        //                else
        //                {
        //                    pushModel.aps.alert.title = OtherNotification.Title;
        //                    pushModel.aps.alert.body = OtherNotification.Text;
        //                    pushModel.notification.NotificationId = OtherNotification.Id;
        //                    pushModel.notification.Type = Type;
        //                }

        //                ApnsServiceBroker apnsBroker;

        //                if (device.ApplicationType == UserDevice.ApplicationTypes.Enterprise)
        //                {
        //                    if (device.EnvironmentType == UserDevice.ApnsEnvironmentTypes.Production)
        //                        apnsBroker = new ApnsServiceBroker(Enterprise.IOS.ProductionConfig);
        //                    else // Sandbox/Development
        //                        apnsBroker = new ApnsServiceBroker(Enterprise.IOS.SandboxConfig);
        //                }
        //                else //PlayStore
        //                {
        //                    if (device.EnvironmentType == UserDevice.ApnsEnvironmentTypes.Production)
        //                        apnsBroker = new ApnsServiceBroker(PlayStore.IOS.ProductionConfig);
        //                    else // Sandbox/Development
        //                        apnsBroker = new ApnsServiceBroker(device.iOSPushConfiguration = PlayStore.IOS.SandboxConfig);
        //                }

        //                apnsBroker.OnNotificationFailed += IOSPushNotificationFailed;
        //                apnsBroker.OnNotificationSucceeded += IOSNotificationSuccess;

        //                // Start the broker
        //                apnsBroker.Start();
        //                apnsBroker.QueueNotification(new ApnsNotification
        //                {
        //                    DeviceToken = device.AuthToken,
        //                    Payload = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(pushModel))
        //                });

        //                apnsBroker.Stop();
        //                apnsBroker.OnNotificationFailed -= IOSPushNotificationFailed;
        //                apnsBroker.OnNotificationSucceeded -= IOSNotificationSuccess;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.LogError(ex);
        //    }
        //}

        private void IOSNotificationSuccess(ApnsNotification notification)
        {

        }

        public async Task SendPushNotifications(List<User> users, string text, string Title)
        {
            try
            {
                foreach (var user in users)
                {
                    var devices = users.SelectMany(x => x.UserDevices.Where(x1 => x1.Platform == true)).ToList();

                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        public async Task SendAndroidPushNotification(List<UserDevice> devices, AdminNotifications AdminNotification = null, Notification OtherNotification = null, int Type = 0)
        {
            try
            {
                if (devices.Count() == 0)//it means their is no device no need to run the below code
                {
                    return;
                }

                NotificationModel msgModel = new NotificationModel();

                foreach (var device in devices.Where(x => x.IsActive))
                {
                    GcmServiceBroker gcmBroker;

                    if (AdminNotification != null)
                    {
                        // msgModel.Type = (int)PushNotificationType.Announcement;
                        var notification = device.User.Notifications.FirstOrDefault(x => x.AdminNotification_Id == AdminNotification.Id);
                        msgModel.Message = AdminNotification.Description;
                        msgModel.NotificationId = notification.Id;
                        msgModel.Title = notification.Title;
                    }
                    else if (OtherNotification != null)
                    {
                        //msgModel.Type = Type;
                        msgModel.Message = OtherNotification.Text;
                        msgModel.NotificationId = OtherNotification.Id;
                        msgModel.Title = OtherNotification.Title;
                    }

                    if (device.ApplicationType == UserDevice.ApplicationTypes.Enterprise)
                    {
                        gcmBroker = new GcmServiceBroker(Enterprise.Android.AndroidGCMConfig);
                    }
                    else
                    {
                        gcmBroker = new GcmServiceBroker(PlayStore.Android.AndroidGCMConfig);
                    }

                    gcmBroker.OnNotificationFailed += AndroidNotificationFailed;
                    gcmBroker.OnNotificationSucceeded += AndroidNotificationSuccess;
                    gcmBroker.Start();

                    gcmBroker.QueueNotification(
                    new GcmNotification
                    {
                        RegistrationIds = new List<string> { device.AuthToken },
                        Priority = GcmNotificationPriority.High,
                        Data = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(msgModel))
                    });

                    gcmBroker.Stop();
                    gcmBroker.OnNotificationFailed -= AndroidNotificationFailed;
                    gcmBroker.OnNotificationSucceeded -= AndroidNotificationSuccess;
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }


        private void AndroidNotificationSuccess(GcmNotification notification)
        {

        }

        private void IOSPushNotificationFailed(ApnsNotification notification, AggregateException exception)
        {
            exception.Handle(ex =>
            {
                // See what kind of exception it was to further diagnose
                if (ex is ApnsNotificationException)
                {
                    var notificationException = (ApnsNotificationException)ex;

                    // Deal with the failed notification
                    var apnsNotification = notificationException.Notification;
                    var statusCode = notificationException.ErrorStatusCode;

                    Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                    if (Convert.ToString(statusCode) == "InvalidToken")
                    {
                        DeleteExpiredSubscription(notification.DeviceToken);
                    }
                }
                else
                {
                    // Inner exception might hold more useful information like an ApnsConnectionException           
                    Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                }

                // Mark it as handled
                return true;
            });
        }

        private void AndroidNotificationFailed(GcmNotification notification, AggregateException exception)
        {
            try
            {
                exception.Handle(ex =>
                {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var notificationException = (GcmNotificationException)ex;

                        // Deal with the failed notification
                        var gcmNotification = notificationException.Notification;
                        var description = notificationException.Description;

                        Console.WriteLine($"GCM Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
                    }
                    else if (ex is GcmMulticastResultException)
                    {
                        var multicastException = (GcmMulticastResultException)ex;

                        foreach (var succeededNotification in multicastException.Succeeded)
                        {
                            Console.WriteLine($"GCM Notification Succeeded: ID={succeededNotification.MessageId}");
                        }

                        foreach (var failedKvp in multicastException.Failed)
                        {
                            var n = failedKvp.Key;
                            var e = failedKvp.Value;

                            Console.WriteLine($"GCM Notification Failed: ID={n.MessageId}, Desc={e.Message}");
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        Console.WriteLine($"Device RegistrationId Expired: {oldId}");
                        DeleteExpiredSubscription(oldId.Trim());

                        if (!String.IsNullOrWhiteSpace(newId))
                        {
                            // If this value isn't null, our subscription changed and we should update our database
                            Console.WriteLine($"Device RegistrationId Changed To: {newId}");
                        }
                    }
                    else if (ex is RetryAfterException)
                    {
                        var retryException = (RetryAfterException)ex;
                        // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                        Console.WriteLine($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
                    }
                    else
                    {
                        Console.WriteLine("GCM Notification Failed for some unknown reason");
                    }

                    // Mark it as handled
                    return true;
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteExpiredSubscription(string AuthToken)
        {
            try
            {
                //using (BasketContext ctx = new BLL())
                //{
                //    bll.Delete(AuthToken);
                //}

                OnDeviceRemoved(new PushNotificationEventArgs(AuthToken));
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected virtual void OnDeviceRemoved(EventArgs e)
        {
            EventHandler handler = DeviceRemoved;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }

    public class PushNotificationEventArgs : EventArgs
    {
        public string AuthToken { get; set; }
        public PushNotificationEventArgs(string authToken)
        {
            AuthToken = authToken;
        }
    }
}
