using System;
using System.Drawing;
using System.IO;
using System.Net;
using log4net;
using Tesseract;
using Rect = System.Windows.Rect;

namespace KajaBot
{
    internal class Ocr
    {
        #region Global variables

        private readonly ILog _log = LogManager.GetLogger("OCR");
        private readonly string _tessdataPath;

        #endregion

        #region Public methods

        public Ocr(string datapath)
        {
            _tessdataPath = datapath;
            _log.Debug("OCR initialized. Tesseract data path: " + datapath);
        }

        public string DownloadAndProcessImage(Uri uri, Rect regionPercentage)
        {
            var engine = new TesseractEngine(_tessdataPath, "hun");

            byte[] imageData;
            try
            {
                var client = new WebClient();
                imageData = client.DownloadData(uri);
                StatisticsCollector.GetInstance().IncrementOutgoingWebRequestCount();

                _log.Debug("Image downloaded from `" + uri + "`. Size: " + imageData.Length + " Byte(s).");
            }
            catch (Exception e)
            {
                _log.Error("Failed to download image from `" + uri + "`. Error: e");
                return "";
            }

            var bitmap = new Bitmap(new MemoryStream(imageData));
            _log.Debug("Image size: " + bitmap.Size);

            // Calculate image region
            var region = new Tesseract.Rect(
                (int) (regionPercentage.Left*bitmap.Width),
                (int) (regionPercentage.Top*bitmap.Height),
                (int) (regionPercentage.Width*bitmap.Width),
                (int) (regionPercentage.Height*bitmap.Height));

            _log.Debug("OCR region: " + region);

            Page page = engine.Process(bitmap, region, PageSegMode.Auto);
            string text = page.GetText();

            _log.Debug("Processed text length: " + text.Length);

            StatisticsCollector.GetInstance().IncrementOcredPageCount();

            return text;
        }

        #endregion
    }
}