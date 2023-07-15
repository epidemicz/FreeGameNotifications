using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FreeGameNotifications
{
    public class FreeGameNotifications : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private FreeGameNotificationsSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("e053a9fe-117f-40fd-ab46-0e09ac442ca9");

        public FreeGameNotifications(IPlayniteAPI api) : base(api)
        {
            settings = new FreeGameNotificationsSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override async void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.            
            logger.Debug("Application Started");

            var games = await EpicGamesWebApi.GetGames();

            logger.Debug($"Found {games.Count} games from EpicGamesWebApi");

            foreach (var game in games)
            {
                logger.Debug(game.Title);

                var notification = new NotificationMessage($"test-{game.Title}", game.Description, NotificationType.Info, () =>
                {
                    System.Diagnostics.Process.Start(game.Url);
                });

                if (!PlayniteApi.Database.Games.Any(i => i.Name == game.Title))
                {
                    PlayniteApi.Notifications.Add(notification);
                }
                else
                {
                    logger.Debug($"{game.Title} appears to already be in game library, skipping notification");
                }
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new FreeGameNotificationsSettingsView();
        }
    }
}