using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KajaBot
{
    class BotCommandInfo : IBotCommand
    {
        public string GetCommand()
        {
            return "$info";
        }

        public string RunAction(string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("KajaBot információk:");

            // Obtain assembly version
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            sb.Append("Verzió: ");
            sb.AppendLine(fvi.FileVersion);

            // Add statistics
            var stats = StatisticsCollector.GetInstance();

            sb.AppendLine();
            sb.AppendLine("Statisztika:");

            sb.AppendFormat("Bejövő üzenetek: {0:D}\n", stats.IncomingMessageCount);
            sb.AppendFormat("Futtatott parancsok: {0:D}\n", stats.ExecutedCommandCount);
            sb.AppendFormat("Étlap lekérdezések: {0:D}\n", stats.ExecutedRestaurantMenuRequestCount);
            sb.AppendFormat("OCR által feldolgozott képek: {0:D}\n", stats.OcredPageCount);
            sb.AppendFormat("Kimenő web-hívások: {0:D}\n", stats.OutgoingWebRequestCount);
            sb.AppendFormat("Ismeretlen parancsok: {0:D}\n", stats.UnknownCommandCount);

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
