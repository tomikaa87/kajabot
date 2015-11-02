using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;
using log4net;

namespace KajaBot
{
    internal class RestaurantKeksz : IRestaurant
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantKeksz");

        public string GetName()
        {
            return "Keksz Bisztró";
        }

        public string GetCommand()
        {
            return "keksz";
        }

        public string GetCurrentMenu()
        {
            const string feedUrl = "https://www.facebook.com/feeds/page.php?format=atom10&id=268952226502652";

            // Download RSS feed
            MemoryStream xmlData;
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Set(HttpRequestHeader.UserAgent,
                    "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0");
                byte[] buf = webClient.DownloadData(new Uri(feedUrl));
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
                xmlData = new MemoryStream(buf);
            }
            catch (Exception)
            {
                return "Hiba: nem lehet letölteni az aktuális étlapot.";
            }

            // Load the RSS feed from the XML
            SyndicationFeed feed = SyndicationFeed.Load(XmlReader.Create(xmlData));

            // Try to find the last menu in the feed
            if (feed != null)
            {
                foreach (SyndicationItem syndicationItem in feed.Items)
                {
                    var textContent = (TextSyndicationContent) syndicationItem.Content;
                    string textLowerCase = textContent.Text.ToLower();

                    // Find the last item which contains some specific words and not older than 2 days
                    if ((textLowerCase.Contains("menü") ||
                         textLowerCase.Contains("főétel") ||
                         textLowerCase.Contains("leves")) &&
                        syndicationItem.PublishDate >= DateTime.Today.Subtract(new TimeSpan(2, 0, 0, 0)))
                    {
                        CultureInfo defaultCulture = Thread.CurrentThread.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("hu-HU");

                        string text = textContent.Text.Replace("<br />", "\n");
                        text = "A (feltételezett) aktuális menü a Kekszben (" +
                               syndicationItem.PublishDate.ToLocalTime().ToString("yyyy. MM. dd. dddd") +
                               "):\n" + text;

                        Thread.CurrentThread.CurrentCulture = defaultCulture;

                        return text;
                    }
                }    
            }

            return "Hiba: nem található az aktuális étlap.";
        }

        public bool Equals(IRestaurant other)
        {
            return other.GetCommand() == GetCommand();
        }
    }
}