using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using log4net;
using Newtonsoft.Json;

namespace KajaBot
{
    internal class ImageSearchResultData
    {
        public string url;
    }

    internal class ImageSearchResponseData
    {
        public ImageSearchResultData[] results;
    }

    internal class ImageSearchResults
    {
        public ImageSearchResponseData responseData;
        public int responseStatus;
    }

    internal class BotCommandImageSearch : IBotCommand
    {
        private readonly ILog _log = LogManager.GetLogger("BotCommandImageSearch");

        public string GetCommand()
        {
            return "imgs";
        }

        public string RunAction(string[] args)
        {
            if (args == null || !args.Any())
                return "mire keressek pontosan?";

            var query = new StringBuilder();
            foreach (var arg in args)
                query.Append(arg + " ");

            _log.Info("Running image search query: " + query.ToString().Trim());

            var uri =
                new Uri("https://ajax.googleapis.com/ajax/services/search/images?v=1.0&rsz=8&q=" +
                        query.ToString().Trim());

            ImageSearchResults results = null;
            try
            {
                var webClient = new WebClient();
                var json = webClient.DownloadString(uri);

                var jsonSerializer = JsonSerializer.Create();
                results = jsonSerializer.Deserialize<ImageSearchResults>(new JsonTextReader(new StringReader(json)));
            }
            catch (Exception e)
            {
                _log.Error("Image search failed. Error: " + e);
            }

            if (results == null || results.responseStatus != 200 || results.responseData.results == null ||
                !results.responseData.results.Any())
                return "sajnos nem találtam semmit. :(";

            var rnd = new Random().Next(results.responseData.results.Length);

            return results.responseData.results[rnd].url;
        }

        public bool Equals(IBotCommand other)
        {
            if (other == null)
                return false;

            return GetCommand() == other.GetCommand();
        }
    }
}