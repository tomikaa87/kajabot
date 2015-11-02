using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace KajaBot
{
    internal class BotCommandTheCodingLove : IBotCommand
    {
        private readonly ILog _log = LogManager.GetLogger("BotCommandTheCodingLove");
        private int _requestCount = 0;

        public string GetCommand()
        {
            return "tcl";
        }

        public string RunAction(string[] args)
        {
            const string errorMessage = "Mára ennyi. Hiba. Menjetek dolgozni!";
            const string warningMessage = "Nem kéne inkább dolgozni? ;)";

            // Only show warning on workdays from 10 to 18
            if (DateTime.Today.DayOfWeek >= DayOfWeek.Monday && DateTime.Today.DayOfWeek <= DayOfWeek.Friday && 
                DateTime.Now.Hour >= 10 && DateTime.Now.Hour < 18)
            {
                ++_requestCount;
                if (_requestCount%10 == 0)
                    return warningMessage;
            }
            
            string html;

            try
            {
                var webCient = new WebClient();
                html = webCient.DownloadString("http://thecodinglove.com/random");
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();

                webCient.Dispose();
            }
            catch (Exception e)
            {
                _log.Error("Failed to get random GIF. Error: " + e);
                return errorMessage;
            }

            //var titleRegex = new Regex(@"\<div.class=.centre.\>.*\<h3\>.*\<a.href.*\>(.*)\<\/a\>.*\<\/h3\>.*\<\/div\>");
            var titleRegex = new Regex(@"\<div.class=.centre.\>.*\<h3\>(.*)\<\/h3\>.*\<\/div\>");
            Match titleMatch = titleRegex.Match(html);

            if (!titleMatch.Success)
            {
                _log.Error("Failed to find title.");
                return errorMessage;
            }

            var pictureLinkRegex = new Regex("\\<div class=\"bodytype\"\\>.{0,50}?\\<img.{0,30}?src=\"(.{0,50}?)\"");
            Match pictureLinkMatch = pictureLinkRegex.Match(html);

            if (!pictureLinkMatch.Success)
            {
                _log.Error("Failed to find picture.");
                return errorMessage;
            }

            var pictureLink = pictureLinkMatch.Groups[1].ToString().Replace(".jpg", ".gif");

            var sb = new StringBuilder();
            sb.AppendLine(titleMatch.Groups[1].ToString());
            sb.AppendLine(pictureLink);

            return sb.ToString();
        }

        public bool Equals(IBotCommand other)
        {
            if (other == null)
                return false;

            return other.GetCommand() == GetCommand();
        }
    }
}