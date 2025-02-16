using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace FreeGameNotifications
{
    public class FreeGameNotifications : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static Timer timer;

        private FreeGameNotificationsSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("e053a9fe-117f-40fd-ab46-0e09ac442ca9");

        public void ResetTimer(int interval)
        {
            if (timer == null)
                CreateTimer();

            if (timer.Enabled)
            {
                timer.Stop();
            }

            timer.Interval = ConvertHourToMillis(interval);
            timer.Enabled = true;
            timer.Start();
        }

        public FreeGameNotifications(IPlayniteAPI api) : base(api)
        {
            Settings = new FreeGameNotificationsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.            
            logger.Debug("Application Started");
            logger.Debug("Initializing new timer");

            // check depending on settings
            CreateTimer();

            // also go ahead and check on startup
            _ = CheckEpicGameStore();
        }

        private void CreateTimer()
        {
            timer = new Timer(ConvertHourToMillis(Settings.Settings.CheckInterval));
            timer.Elapsed += (Object source, ElapsedEventArgs e) =>
            {
                _ = CheckEpicGameStore();
            };

            timer.Enabled = true;
            timer.Start();
        }

        private int ConvertHourToMillis(int interval)
        {
            return 1000 * 60 * 60 * interval;
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
            if (timer.Enabled)
            {
                timer.Stop();
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new FreeGameNotificationsSettingsView();
        }

        public async Task CheckEpicGameStore()
        {
            var games = await EpicGamesWebApi.GetGames();

            logger.Debug($"Found {games.Count} games from EpicGamesWebApi");

            foreach (var game in games)
            {
                if (Settings.Settings.UseNotificationHistory && Settings.Settings.History.Contains(game.Title))
                {
                    logger.Debug($"{game.Title} : notification ignored because it is in history");
                    continue;
                }

                if (!Settings.Settings.AlwaysShowNotifications && PlayniteApi.Database.Games.Any(i => i.Name == game.Title))
                {
                    logger.Debug($"{game.Title} : notification ignored because it is in the library");
                    continue;
                }

                logger.Debug($"{game.Title} : showing notification");

                var notification = new NotificationMessage($"{game.Title}", game.Description, NotificationType.Info, () =>
                {
                    System.Diagnostics.Process.Start(game.Url);
                });

                PlayniteApi.Notifications.Add(notification);

                if (Settings.Settings.UseNotificationHistory)
                {
                    logger.Debug($"{game.Title} : adding to history");
                    Settings.Settings.History.Add(game.Title);
                    this.SavePluginSettings(Settings.Settings);
                }
            }
        }
    }
}