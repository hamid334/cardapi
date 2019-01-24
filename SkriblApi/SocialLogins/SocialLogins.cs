using BasketApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace BasketApi
{
    public class SocialLogins
    {
        public async Task<SocialUserViewModel> GetSocialUserData(string access_token, SocialLoginType socialLoginType)
        {
            try
            {
                HttpClient client = new HttpClient();
                string urlProfile = "";

                if (socialLoginType == SocialLoginType.Google)
                    urlProfile = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + access_token;
                else if (socialLoginType == SocialLoginType.Facebook)
                    urlProfile = "https://graph.facebook.com/me?access_token=" + access_token;

                client.CancelPendingRequests();
                HttpResponseMessage output = await client.GetAsync(urlProfile);

                if (output.IsSuccessStatusCode)
                {
                    string outputData = await output.Content.ReadAsStringAsync();
                    SocialUserViewModel socialUser = JsonConvert.DeserializeObject<SocialUserViewModel>(outputData);

                    return socialUser;
                    //if (socialUser != null)
                    //{
                    //    // You will get the user information here.
                    //}
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public enum SocialLoginType
        {
            Google,
            Facebook,
            Instagram,
            Twitter
        }
    }
}