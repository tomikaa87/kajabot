using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using log4net;

namespace KajaBot
{
    internal class RestaurantVian : IRestaurant
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantVian");

        public string GetName()
        {
            return "Cafe Vian Gozsdu";
        }

        public string GetCommand()
        {
            return "vian";
        }

        public string GetCurrentMenu()
        {
            // Download the home page and try to find the menu link
            string homePageContent = "";
            try
            {
                var webClient = new WebClient();
                homePageContent = webClient.DownloadString("http://cafevian.com/hu/");
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
            }
            catch (Exception e)
            {
                _log.Error("Failed to download home page of Vian. Error: " + e);
                return "Hiba: nem lehet letölteni az aktuális étlapot.";
            }

            var regex = new Regex(@"(http:\/\/cafevian\.com\/wp\-content\/uploads\/.*\.pdf).*GOZSDU");
            Match match = regex.Match(homePageContent);

            if (!match.Success)
            {
                _log.Error("Failed to find Vian's menu PDF URL");
                return "Hiba: nem lehet letölteni az aktuális étlapot.";
            }

            // Download the menu page
            MemoryStream pdfContent = null;
            try
            {
                var webClient = new WebClient();
                byte[] data = webClient.DownloadData(new Uri(match.Groups[1].ToString()));
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
                pdfContent = new MemoryStream(data);
            }
            catch (Exception e)
            {
                _log.Error("Failed to download menu of Vian. Error: " + e);
                return "Hiba: nem lehet letölteni az aktuális étlapot.";
            }

            // Convert PDF to plain text
            var pdf = new PdfReader(pdfContent);
            string text = PdfTextExtractor.GetTextFromPage(pdf, 1);

            var dayOfWeek = (int) DateTime.Today.DayOfWeek;
            string[] dayNames = DateTimeFormatInfo.GetInstance(new CultureInfo("hu-HU")).DayNames;

            // Find the menu for today
            text = text.Replace("\r", " ");
            string[] lines = text.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Napi menü a Vianban:");

            bool menuFoundForToday = false;

            for (int i = 0; i < lines.Count(); ++i)
            {
                string line = lines[i];
                line = line.Replace("\n", "");

                if (!line.ToLower().Contains(dayNames[dayOfWeek] + " "))
                    continue;

                menuFoundForToday = true;

                line = line.Trim();

                string nextDayName = dayNames[(dayOfWeek + 1)%7];

                for (; i < lines.Count(); ++i)
                {
                    line = lines[i];
                    line = line.Replace("\n", "");
                    line = line.Trim();

                    if (line.ToLower().Contains(nextDayName))
                        break;
                    stringBuilder.AppendLine(line);
                }
            }

            if (!menuFoundForToday)
                return "Erre a napra nincs ebéd menü a Vianban.";

            return stringBuilder.ToString();
        }

        public bool Equals(IRestaurant other)
        {
            return other.GetCommand() == GetCommand();
        }
    }
}