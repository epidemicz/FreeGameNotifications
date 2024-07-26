using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace FreeGameNotifications
{
    public class FreeGameNotificationsSettings : ObservableObject
    {
        private bool alwaysShowNotifications = false;
        private int checkInterval = 6; // in hours ; minimum is every 1 hour
        private bool useNotificationHistory = true;
        private List<string> history = new List<string>();


        public bool AlwaysShowNotifications { get => alwaysShowNotifications; set => SetValue(ref alwaysShowNotifications, value); }
        public int CheckInterval { get => checkInterval; set => SetValue(ref checkInterval, value); }
        public bool UseNotificationHistory { get => useNotificationHistory; set => SetValue(ref useNotificationHistory, value); }
        public List<string> History { get => history; set => SetValue(ref history, value); }
    }

    public class FreeGameNotificationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly FreeGameNotifications plugin;
        private FreeGameNotificationsSettings EditingClone { get; set; }

        private FreeGameNotificationsSettings settings;
        public FreeGameNotificationsSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public FreeGameNotificationsSettingsViewModel(FreeGameNotifications plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<FreeGameNotificationsSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new FreeGameNotificationsSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();

            if (settings.CheckInterval < 1)
            {
                settings.CheckInterval = 1;
            }
            else if (settings.CheckInterval > 24)
            {
                settings.CheckInterval = 24;
            }

            this.plugin.ResetTimer(settings.CheckInterval);

            _ = this.plugin.CheckEpicGameStore();

            return true;
        }
    }
}