using System.Text;

namespace KajaBot
{
    internal class BotCommandLogHistory : IBotCommand
    {
        public string GetCommand()
        {
            return "$log";
        }

        public string RunAction(string[] args)
        {
            var sb = new StringBuilder();
            string[] history = LoggingModel.GetInstance().History;

            sb.AppendLine("Az utolsó " + history.Length + " elem a naplóból:");

            foreach (string s in history)
                sb.AppendLine(s);

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