using System.Windows;

namespace KajaBot
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Bot _bot;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            StatisticsCollector.GetInstance();
            LoggingModel.GetInstance();

            var mainWindow = new MainWindow();
            mainWindow.Show();

            _bot = new Bot("<your-slack-api-token-comes-here>");
            _bot.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _bot.Stop();

            base.OnExit(e);
        }
    }
}