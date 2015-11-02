using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using log4net;

namespace KajaBot
{
    internal class RestaurantNoir : IRestaurant
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantNoir");

        public string GetName()
        {
            return "noir et l'or";
        }

        public string GetCommand()
        {
            return "noir";
        }

        public string GetCurrentMenu()
        {
            string content = "";

            // Download the menu page
            try
            {
                var webClient = new WebClient();
                content = webClient.DownloadString(new Uri("http://noiretlor.hu/napi-menu"));
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
            }
            catch (Exception e)
            {
                _log.Error("Failed to download menu of Noir. Error: " + e);
                return "Hiba: nem lehet letölteni az aktuális étlapot.";
            }

            // Try to find the URI of the menu image
            var regex = new Regex(@".*(\/upload\/image\/.*\.jpg)");
            Match match = regex.Match(content);

            if (!match.Success)
                return "Hiba: nem található a napi menü URL-je.";

            var imageUri = new Uri("http://noiretlor.hu" + match.Groups[1]);

            // Calculate OCR region
            var region = new Rect {X = 0, Y = 0.216, Width = 1, Height = 0.46};

            string text = "";

            // Download and process the image
            try
            {
                var ocr = new Ocr(AppDomain.CurrentDomain.BaseDirectory + "TesseractData");
                text = ocr.DownloadAndProcessImage(imageUri, region);
            }
            catch (Exception e)
            {
                _log.Error("Failed to process menu image. Error: " + e);
            }

            return "Heti menü a Noirban:\n" + text;
        }

        public bool Equals(IRestaurant other)
        {
            return other.GetCommand() == GetCommand();
        }
    }
}