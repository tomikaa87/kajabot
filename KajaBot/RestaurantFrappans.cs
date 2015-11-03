using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace KajaBot
{
    class RestaurantFrappans : IRestaurant
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantFrappans");

        public bool Equals(IRestaurant other)
        {
            return other.GetCommand() == GetCommand();
        }

        public string GetName()
        {
            return "Frappans Etterem";
        }

        public string GetCommand()
        {
            return "frappans";
        }

        public string GetCurrentMenu()
        {
            try
            {
                string menuRequest = CreateMenuRequest("frappans");
                string menuResponse = MakeRequest(menuRequest);
                return GetMenuFromJson(menuResponse);

            }
            catch (Exception)
            {
                return "Nem sikerult letolteni az etlapot.";

            }
        }

        public string CreateMenuRequest(string restaurantName)
        {
            string UrlRequest = "https://graph.facebook.com/v2.5/" +
                                restaurantName +
                                "/posts?access_token=" +
                                ""; //Your FB API Token Here //TODO put this into a config file
            return UrlRequest;

        }

        public string MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _log.Error("Server Error (HTTP: " +
                               response.StatusCode +
                               " : " + response.StatusDescription);
                        return null;
                    }
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        return reader.ReadToEnd();

                    }
                }
            }
            catch (Exception)
            {
                _log.Error("Check your facebook access token.");
                return null;
            }
        }

        public string GetMenuFromJson(string json)
        {
            FrappansPosts posts = JsonConvert.DeserializeObject<FrappansPosts>(json);
            var postWithLastMenu = posts.data
                .Where(post => post.message.Contains("Heti menü"))
                .Select(post => post);
            var firstPost = postWithLastMenu.First().message;
            return firstPost;
        }

    }

    public class FrappansPosts
    {
        public Datum[] data { get; set; }
        public Paging paging { get; set; }
    }

    public class Paging
    {
        public string previous { get; set; }
        public string next { get; set; }
    }

    public class Datum
    {
        public string message { get; set; }
        public DateTime created_time { get; set; }
        public string id { get; set; }
        public string story { get; set; }
    }

}
