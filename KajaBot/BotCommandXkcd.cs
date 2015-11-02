using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using log4net;

namespace KajaBot
{
    internal class BotCommandXkcd : IBotCommand
    {
        private readonly ILog _log = LogManager.GetLogger("BotCommandXkcd");

        public string GetCommand()
        {
            return "xkcd";
        }

        public string RunAction(string[] args)
        {
            string xhtmlData;

            try
            {
                var webClient = new WebClient();
                xhtmlData = webClient.DownloadString("https://c.xkcd.com/random/comic/");
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
            }
            catch (Exception e)
            {
                _log.Error("Failed to download the web page. Error: " + e);
                return "Hiba: nem lehet letölteni az XKCD honlapját.";
            }

            // We must strip down the html code to avoid read errors
            int pos = xhtmlData.IndexOf("<div id=\"comic\">", StringComparison.OrdinalIgnoreCase);
            if (pos > -1)
            {
                xhtmlData = xhtmlData.Remove(0, pos);
                pos = xhtmlData.IndexOf("<img", StringComparison.OrdinalIgnoreCase);
                if (pos > -1)
                    xhtmlData = xhtmlData.Remove(0, pos);
            }

            pos = xhtmlData.IndexOf("</div>", StringComparison.CurrentCultureIgnoreCase);
            if (pos > -1)
                xhtmlData = xhtmlData.Remove(pos);

            var reader = new StringReader(xhtmlData /*match.Groups[1].ToString()*/);
            XmlReader xml = XmlReader.Create(reader);

            try
            {
                while (xml.Read())
                {
                    if (xml.NodeType != XmlNodeType.Element)
                        continue;

                    if (xml.Name != "img" || !xml.HasAttributes)
                        continue;

                    string src = xml.GetAttribute("src");

                    if (src == null)
                    {
                        _log.Error("Failed to find comic's src.");
                        return "Hiba: nem található a képregény forrása.";
                    }

                    string title = xml.GetAttribute("title");
                    string alt = xml.GetAttribute("alt");

                    var sb = new StringBuilder();
                    if (alt != null)
                        sb.AppendFormat("*{0:G}*\n", alt);
                    if (title != null)
                        sb.AppendLine(title);
                    sb.AppendFormat("http:{0:G}", src);

                    return sb.ToString();
                }
            }
            catch (XmlException e)
            {
                if (!e.Message.ToLower(CultureInfo.InvariantCulture).Contains("unexpected end tag"))
                {
                    _log.Error("Failed to parse XHTML. Error: " + e);
                    return "Hiba a feldolgozás során. Ellenőrizd a naplót.";
                }
            }

            _log.Error("Error.");
            return "Ez most nem fog menni.";
        }

        public bool Equals(IBotCommand other)
        {
            if (other == null)
                return false;

            return other.GetCommand() == GetCommand();
        }
    }
}