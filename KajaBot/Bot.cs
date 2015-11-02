using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace KajaBot
{
    internal class Bot
    {
        #region Global variables

        private readonly string _apiKeyToken;
        private readonly List<IBotCommand> _commands = new List<IBotCommand>();
        private readonly ILog _log = LogManager.GetLogger("Bot");
        private readonly RestaurantManager _restaurantManager;
        private LoginResponse _loginResponse;
        private SlackSocketClient _slackClient;
        private readonly Timer _keepAliveTimer;

        #endregion

        #region Public methods

        public Bot(string apiKey)
        {
            _apiKeyToken = apiKey;
            _restaurantManager = new RestaurantManager();

            _keepAliveTimer = new Timer(OnKeepAliveTimerTick);

            SetupCommands();
            SetupRestaurants();
        }

        public void Start()
        {
            if (_slackClient != null)
                return;

            _log.Info("Starting...");

            _slackClient = new SlackSocketClient(_apiKeyToken);
            _slackClient.OnHello += _slackClient_OnHello;
            _slackClient.Connect(_slackClient_OnConnected);

            _keepAliveTimer.Change(60000, 60000);
        }

        public void Stop()
        {
            _log.Info("Shutting down...");
            _slackClient = null;
        }

        #endregion

        #region Event handling

        private void _slackClient_OnConnected(LoginResponse obj)
        {
            _log.Info("Connected");
            _loginResponse = obj;
        }

        private void _slackClient_OnHello()
        {
            _log.Debug("Hello message received");

            _slackClient.BindCallback<NewMessage>(OnSlackMessageReceived);
            _slackClient.BindCallback<Typing>(typing => { });
        }

        private void OnSlackMessageReceived(NewMessage obj)
        {
            if (!IsMessageForMe(obj))
                return;

            StatisticsCollector.GetInstance().IncrementIncomingMessageCount();

            // Find sender user name
            string senderName = "<unknown>";
            foreach (User user in _loginResponse.users)
            {
                if (user.id == obj.user)
                {
                    senderName = user.name;
                    break;
                }
            }

            _log.Info("Incoming message from " + senderName + " for me: " + obj.text);

            ParseBotMessage(obj);
        }

        private void OnKeepAliveTimerTick(object obj)
        {
            if (_slackClient == null)
                return;

            if (!_slackClient.IsConnected)
            {
                _log.Warn("Reconnecting to Slack");
                _slackClient.Connect(_slackClient_OnConnected);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Checks if the message is addressed to the bot.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool IsMessageForMe(NewMessage msg)
        {
            // Check if the message is directly for me
            if (msg.channel.StartsWith("D") && msg.user != _loginResponse.self.id)
                return true;

            // Check if the message begins with my user ID
            var re = new Regex(@"^\<@(.*)\>(.*)$");
            Match m = re.Match(msg.text);
            return (m.Success && m.Groups[1].Value == _loginResponse.self.id);
        }

        /// <summary>
        ///     Processes the message sent to the bot and runs the command if it's valid.
        /// </summary>
        /// <param name="msg"></param>
        private void ParseBotMessage(NewMessage msg)
        {
            string text = msg.text.ToLower().Trim();            
            var words = text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            string command = "";

            if (words.Any())
            {
                command = words[0];
                string userId = "<@" + _loginResponse.self.id.ToLower() + ">";
                command = command.Replace(userId + ":", "");
                command = command.Replace(userId, "");

                if (command.Length == 0)
                    words = words.Skip(1).ToArray();
                
                if (words.Any())
                    command = words[0];
            }
            
            command = command.Trim();

            string[] args = null;
            if (words.Count() > 1)
                args = words.Skip(1).ToArray();

            string targetUser = "<@" + msg.user + ">";
            string answer = targetUser + ": ";

            if (_restaurantManager.ContainsRestaurant(command))
            {
                answer += _restaurantManager.GetMenu(command);
            }
            else if (IsCommandKnown(command))
            {
                answer += RunCommand(command, args);
            }
            else
            {
                StatisticsCollector.GetInstance().IncrementUnknownCommandCount();
                answer = GetRandomUnknownCommandAnswer(targetUser);
            }

            if (answer.Length > 0)
                _slackClient.SendMessage(received => { }, msg.channel, answer);
        }

        /// <summary>
        ///     Returns a random funny string
        /// </summary>
        /// <returns></returns>
        private static string GetRandomUnknownCommandAnswer(string targetUser)
        {
            var answers = new[]
            {
                "Kedves $1, sajnos erre nem tudok mit mondani.",
                "Sajnálom $1, de ezt nem értem.",
                "Igazán restellem, kedves $1, de ezzel nem tudok mit kezdeni.",
                "$1: Segmentation fault"
            };

            var rnd = new Random();

            var answer = answers[rnd.Next(answers.Length)];
            answer = answer.Replace("$1", targetUser);
            return answer;
        }

        /// <summary>
        ///     Adds known restaurants to the manager.
        /// </summary>
        private void SetupRestaurants()
        {
            _restaurantManager.AddRestaurant(new RestaurantNoir());
            _restaurantManager.AddRestaurant(new RestaurantVian());
            _restaurantManager.AddRestaurant(new RestaurantKeksz());
            _restaurantManager.AddRestaurant(new RestaurantVarju());
        }

        /// <summary>
        ///     Adds known commands to the bot.
        /// </summary>
        private void SetupCommands()
        {
            AddCommand(new BotCommandListRestaurants(_restaurantManager));
            AddCommand(new BotCommandLogHistory());
            AddCommand(new BotCommandInfo());
            AddCommand(new BotCommandBreakfast());
            AddCommand(new BotCommand9Gag());
            AddCommand(new BotCommandTheCodingLove());
            AddCommand(new BotCommandXkcd());
            AddCommand(new BotCommandImageSearch());
        }

        /// <summary>
        ///     Adds a new command to the bot.
        /// </summary>
        /// <param name="command"></param>
        private void AddCommand(IBotCommand command)
        {
            if (_commands.Contains(command))
                return;

            _log.Info("Command added: " + command.GetCommand());

            _commands.Add(command);
        }

        /// <summary>
        ///     Checks if the given command is known by the bot.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool IsCommandKnown(string command)
        {
            IBotCommand cmd =
                _commands.Find(x => x.GetCommand().Equals(command, StringComparison.OrdinalIgnoreCase));
            return cmd != null;
        }

        /// <summary>
        ///     Runs the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string RunCommand(string command, string[] args)
        {
            IBotCommand cmd =
                _commands.Find(x => x.GetCommand().Equals(command, StringComparison.OrdinalIgnoreCase));
            if (cmd == null)
                return "Hiba: nincs ilyen parancs.";

            _log.Info("Running command: " + command);

            StatisticsCollector.GetInstance().IncrementExecutedCommandCount();

            return cmd.RunAction(args);
        }

        #endregion
    }
}