﻿using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeGameNotifications
{
    public class FreeGameNotificationsSettings : ObservableObject
    {
        private string option1 = string.Empty;
        private bool alwaysShowNotifications = false;
        private bool optionThatWontBeSaved = false;
        private int checkInterval = 3600000;

        public string Option1 { get => option1; set => SetValue(ref option1, value); }
        public bool AlwaysShowNotifications { get => alwaysShowNotifications; set => SetValue(ref alwaysShowNotifications, value); }
        public int CheckInterval { get => checkInterval; set => SetValue(ref checkInterval, value); }

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        [DontSerialize]
        public bool OptionThatWontBeSaved { get => optionThatWontBeSaved; set => SetValue(ref optionThatWontBeSaved, value); }
    }

    public class FreeGameNotificationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly FreeGameNotifications plugin;
        private FreeGameNotificationsSettings editingClone { get; set; }

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
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
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

            if (settings.CheckInterval < 60000)
            {
                settings.CheckInterval = 60000;
            }

            this.plugin.ResetTimer(settings.CheckInterval);

            _ = this.plugin.CheckEpicGameStore();

            return true;
        }
    }
}