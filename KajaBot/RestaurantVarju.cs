using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;

namespace KajaBot
{
    internal class RestaurantVarju : IRestaurant
    {
        private readonly ILog _log = LogManager.GetLogger("RestaurantVarju");

        public string GetName()
        {
            return "VakVarjú étterem";
        }

        public string GetCommand()
        {
            return "varju";
        }

        public string GetCurrentMenu()
        {
            string xhtmlData;

            try
            {
                var webClient = new WebClient();
                xhtmlData = webClient.DownloadString("http://pest.vakvarju.com/hu/napimenu");
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();
            }
            catch (Exception e)
            {
                _log.Error("Failed to download the web page. Error: " + e);
                return "Hiba: nem lehet letölteni a VakVarjú honlapját.";
            }

            // We must strip down the html code to avoid read errors
            int pos = xhtmlData.IndexOf("<div id=\"etlapfelsorol\">", StringComparison.InvariantCultureIgnoreCase);
            if (pos > -1)
                xhtmlData = xhtmlData.Remove(0, pos);

            pos = xhtmlData.IndexOf("<div class=\"footerbox\">", StringComparison.CurrentCultureIgnoreCase);
            if (pos > -1)
                xhtmlData = xhtmlData.Remove(pos);

            // Poor man's sanitizer...
            xhtmlData = ToUtf8(xhtmlData);
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&iacute;", "í");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&aacute;", "á");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&eacute;", "é");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&uacute;", "ú");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&oacute;", "ó");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&ouml;", "ö");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&uuml;", "ü");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&bdquo;", "\"");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&rdquo;", "\"");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&nbsp;", " ");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "&ndash;", "-");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "<br />", "\n");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "<br/>", "\n");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "<h2>", "");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "</h2>", "");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "<p>", "");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "</p>", "");
            xhtmlData = ReplaceCaseInsensitive(xhtmlData, "\t", "");

            var reader = new StringReader(xhtmlData);

            var settings = new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore};
            XmlReader xml = XmlReader.Create(reader, settings);

            string menuText = null;

            var dayOfWeek = (int) DateTime.Today.DayOfWeek;
            string[] dayNames = DateTimeFormatInfo.GetInstance(new CultureInfo("hu-HU")).DayNames;

            try
            {
                while (xml.Read())
                {
                    if (xml.NodeType != XmlNodeType.Element)
                        continue;

                    if (xml.Name != "div" || !xml.HasAttributes || xml.GetAttribute("class") != "nev")
                        continue;

                    string headerContent = xml.ReadElementContentAsString();

                    // Read the menu contents if we have found one for today
                    if (!headerContent.ToLower().Contains(dayNames[dayOfWeek].ToLower()))
                        continue;

                    while (xml.Read())
                    {
                        if (xml.NodeType != XmlNodeType.Element)
                            continue;

                        if (xml.Name != "div" || !xml.HasAttributes || xml.GetAttribute("class") != "text")
                            continue;

                        string content = xml.ReadElementContentAsString();

                        var sb = new StringBuilder();
                        sb.AppendLine("A mai menü a Varjúban:");
                        sb.AppendLine(headerContent);
                        sb.AppendLine();
                        sb.AppendLine(content);

                        menuText = sb.ToString();

                        break;
                    }
                }
            }
            catch (XmlException e)
            {
                if (!e.Message.ToLower().Contains("unexpected end tag"))
                {
                    _log.Error("Failed to parse XHTML. Error: " + e);
                    menuText = "Hiba a feldolgozás során. Ellenőrizd a naplót.";
                }
            }

            if (menuText == null)
                return "A mai napra nem található menü a Varjúban.";

            return menuText;
        }

        public bool Equals(IRestaurant other)
        {
            return other.GetCommand() == GetCommand();
        }

        public static string ReplaceCaseInsensitive(string str, string findMe, string newValue)
        {
            return Regex.Replace(str,
                Regex.Escape(findMe),
                Regex.Replace(newValue, "\\$[0-9]+", @"$$$0"),
                RegexOptions.IgnoreCase);
        }

        private string ToUtf8(string input)
        {
            byte[] bytes = Encoding.Default.GetBytes(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}