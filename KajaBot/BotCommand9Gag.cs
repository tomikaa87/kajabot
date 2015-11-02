using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using log4net;

namespace KajaBot
{
    internal class BotCommand9Gag : IBotCommand
    {
        private readonly ILog _log = LogManager.GetLogger("BotCommand9Gag");

        public string GetCommand()
        {
            return "9gag";
        }

        public string RunAction(string[] args)
        {
            string feedUrl = "http://9gag-rss.com/api/rss/get?code=9GAGHot&format=1";

            if (args != null && args.Any())
            {
                if (args[0].ToLower() == "nsfw")
                    feedUrl = "http://9gag-rss.com/api/rss/get?code=9GAGNSFW&format=1";
                else if (args[0].ToLower() == "gif")
                    feedUrl = "http://9gag-rss.com/api/rss/get?code=9GAGGIF&format=1";
            }

            MemoryStream feedDataStream;
            try
            {
                var webClient = new WebClient();
                byte[] buf = webClient.DownloadData(feedUrl);
                webClient.Dispose();

                feedDataStream = new MemoryStream(buf);
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();

            }
            catch (Exception e)
            {
                _log.Error("Failed to download 9Gag RSS. Error: " + e);
                return "It's dead, Jim. :( Check the log for details.";
            }


            XmlReader xml = XmlReader.Create(feedDataStream);

            SyndicationFeed feed;
            try
            {
                feed = SyndicationFeed.Load(xml);
            }
            catch (Exception e)
            {
                return "No gags for today. Failed to load syndication feed.";
            }
                
            feedDataStream.Dispose();

            if (feed == null || !feed.Items.Any())
                return "No gags for today. (RSS parse error)";

            IEnumerator<SyndicationItem> enumerator = feed.Items.GetEnumerator();

            int rnd = new Random().Next(feed.Items.Count()) + 1;
            while (rnd-- > 0)
                enumerator.MoveNext();

            SyndicationItem item = enumerator.Current;
            var content = (TextSyndicationContent) item.Content;

            return item.Id;
        }

        public bool Equals(IBotCommand other)
        {
            if (other == null)
                return false;

            return other.GetCommand() == GetCommand();
        }
    }
}