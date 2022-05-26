﻿using Semver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using VolumeControl.Attributes;
using VolumeControl.Audio;
using VolumeControl.Core;
using VolumeControl.Core.Extensions;
using VolumeControl.Helpers.Addon;
using VolumeControl.Hotkeys;
using VolumeControl.Hotkeys.Addons;
using VolumeControl.Log.Enum;
using VolumeControl.Win32;
using VolumeControl.WPF;
using static VolumeControl.Audio.AudioAPI;

namespace VolumeControl.Helpers
{
    public class VolumeControlSettings : INotifyPropertyChanged, INotifyPropertyChanging, INotifyCollectionChanged, IDisposable
    {
        public VolumeControlSettings()
        {
            AppDomain? appDomain = AppDomain.CurrentDomain;
            ExecutablePath = Path.Combine(appDomain.RelativeSearchPath ?? appDomain.BaseDirectory, Path.ChangeExtension(appDomain.FriendlyName, ".exe"));
            _registryRunKeyHelper = new();

            _audioAPI = new();
            _hWndMixer = WindowHandleGetter.GetWindowHandle();
            
            var assembly = Assembly.GetAssembly(typeof(VolumeControlSettings));
            VersionNumber = assembly?.GetCustomAttribute<AssemblyAttribute.ExtendedVersion>()?.Version ?? string.Empty;
            ReleaseType = assembly?.GetCustomAttribute<ReleaseType>()?.Type ?? ERelease.NONE;

            Version = VersionNumber.GetSemVer() ?? new(0, 0, 0);
            AddonManager = new(this);

            HotkeyActionManager actionManager = new();
            // Add premade actions
            actionManager.Types.Add(typeof(AudioAPIActions));
            actionManager.Types.Add(typeof(WindowsAPIActions));

            List<IBaseAddon> addons = new()
            {
                actionManager
            };
            // Load addons
            AddonManager.LoadAddons(ref addons);
            // Retrieve a list of all loaded action names
            ActionNames = actionManager
                .GetActionNames()
                .Where(s => s.Length > 0)
                .OrderBy(s => s[0])
                .ToList();
            // Create the hotkey manager
            _hotkeyManager = new(actionManager);
            _hotkeyManager.LoadHotkeys();

            // Initialize the addon API
            API.Internal.Initializer.Initialize(_audioAPI, _hotkeyManager, _hWndMixer);

            Log.Info($"Volume Control v{VersionNumber} Initializing...");

            // v Load Settings v //
            ShowIcons = Settings.ShowIcons;
            AdvancedHotkeyMode = Settings.AdvancedHotkeys;
            RunAtStartup = Settings.RunAtStartup;
            StartMinimized = Settings.StartMinimized;
            CheckForUpdates = Settings.CheckForUpdatesOnStartup;
            NotificationEnabled = Settings.NotificationEnabled;
            NotificationTimeout = Settings.NotificationTimeoutInterval;
            // ^ Load Settings ^ //

            Log.Debug($"{nameof(VolumeControlSettings)} finished initializing settings from all assemblies.");

            _audioAPI.PropertyChanged += Handle_AudioAPI_PropertyChanged;            
        }
        private void SaveSettings()
        {
            // v Save Settings v //
            Settings.ShowIcons = ShowIcons;
            Settings.AdvancedHotkeys = AdvancedHotkeyMode;
            Settings.RunAtStartup = RunAtStartup;
            Settings.StartMinimized = StartMinimized;
            Settings.CheckForUpdatesOnStartup = CheckForUpdates;
            Settings.NotificationEnabled = NotificationEnabled;
            Settings.NotificationTimeoutInterval = NotificationTimeout;
            // ^ Save Settings ^ //

            // VolumeControl
            Settings.Save();
            Settings.Reload();

            // VolumeControl.Audio
            AudioSettings.Save();
            AudioSettings.Reload();

            // VolumeControl.Hotkeys
            HotkeySettings.Save();
            HotkeySettings.Reload();

            // VolumeControl.Log
            LogSettings.Save();
            LogSettings.Reload();

            Log.Debug($"{nameof(VolumeControlSettings)} finished saving settings from all assemblies.");
        }

        #region Events
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => ((INotifyCollectionChanged)_hotkeyManager).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)_hotkeyManager).CollectionChanged -= value;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        protected virtual void NotifyPropertyChanging([CallerMemberName] string propertyName = "") => PropertyChanging?.Invoke(this, new(propertyName));
        #endregion Events

        #region Fields
        #region PrivateFields
        private bool disposedValue;
        private readonly AudioAPI _audioAPI;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IntPtr _hWndMixer;
        private readonly RunKeyHelper _registryRunKeyHelper;
        private IEnumerable<string>? _targetAutoCompleteSource;
        #endregion PrivateFields
        public readonly AddonManager AddonManager;
        public readonly string ExecutablePath;
        public readonly ERelease ReleaseType;
        #endregion Fields

        #region Properties
        #region Other
        /// <summary>
        /// This is used by the target box's autocomplete feature, and is automatically invalidated & refreshed each time the sessions list changes.
        /// </summary>
        public IEnumerable<string> TargetAutoCompleteSource => _targetAutoCompleteSource ??= AudioAPI.GetSessionNames(SessionNameFormat.ProcessIdentifier | SessionNameFormat.ProcessName);
        public IEnumerable<string> ActionNames
        {
            get => _actionNames;
            internal set
            {
                NotifyPropertyChanging();
                _actionNames = value;
                NotifyPropertyChanged();
            }
        }
        private IEnumerable<string> _actionNames = null!;
        public IEnumerable<string> NotificationModes
        {
            get => _notificationModes;
            set
            {
                NotifyPropertyChanging();
                _notificationModes = value;
                NotifyPropertyChanged();
            }
        }
        private IEnumerable<string> _notificationModes = Enum.GetNames(typeof(ListNotificationDisplayTarget));
        #endregion Other

        #region Statics
        #region PrivateStatics
        /// <summary>Static accessor for <see cref="Properties.Settings.Default"/>.</summary>
        private static Properties.Settings Settings => Properties.Settings.Default;
        /// <summary>Static accessor for <see cref="HotkeyManagerSettings.Default"/>.</summary>
        private static HotkeyManagerSettings HotkeySettings => HotkeyManagerSettings.Default;
        /// <summary>Static accessor for <see cref="AudioAPISettings.Default"/>.</summary>
        private static AudioAPISettings AudioSettings => AudioAPISettings.Default;
        /// <summary>Static accessor for <see cref="Log.Properties.Settings.Default"/>.</summary>
        private static Log.Properties.Settings LogSettings => global::VolumeControl.Log.Properties.Settings.Default;
        private static Log.LogWriter Log => global::VolumeControl.Log.FLog.Log;
        #endregion PrivateStatics

        public string VersionNumber { get; private set; }
        public SemVersion Version { get; private set; }
        #endregion Statics

        #region ParentObjects
        public AudioAPI AudioAPI => _audioAPI;
        public HotkeyManager HotkeyAPI => _hotkeyManager;
        #endregion ParentObjects

        #region Settings
        /// <summary>
        /// Gets or sets a boolean that determines whether or not device/session icons are shown in the UI.
        /// </summary>
        public bool ShowIcons
        {
            get => _showIcons;
            set
            {
                NotifyPropertyChanging();
                _showIcons = value;
                NotifyPropertyChanged();
            }
        }
        private bool _showIcons;
        /// <summary>
        /// Gets or sets the hotkey editor mode, which can be either false (basic mode) or true (advanced mode).
        /// </summary>
        /// <remarks>Advanced mode allows the user to perform additional actions in the hotkey editor:
        /// <list type="bullet">
        /// <item><description>Create and delete hotkeys.</description></item>
        /// <item><description>Change the action bindings of hotkeys.</description></item>
        /// <item><description>Rename hotkeys.</description></item>
        /// </list></remarks>
        public bool AdvancedHotkeyMode
        {
            get => _advancedHotkeyMode;
            set
            {
                NotifyPropertyChanging();
                _advancedHotkeyMode = value;
                NotifyPropertyChanged();
            }
        }
        private bool _advancedHotkeyMode;
        public bool RunAtStartup
        {
            get => _registryRunKeyHelper.CheckRunAtStartup(Settings.RegistryStartupValueName, ExecutablePath);
            set
            {
                try
                {
                    NotifyPropertyChanging();

                    if (value)
                    {
                        _registryRunKeyHelper.EnableRunAtStartup(Settings.RegistryStartupValueName, ExecutablePath);
                        Log.Conditional(new(EventType.DEBUG, $"Enabled Run at Startup: {{ Value: {Settings.RegistryStartupValueName}, Path: {ExecutablePath} }}"), new(EventType.INFO, "Enabled Run at Startup."));
                    }
                    else
                    {
                        _registryRunKeyHelper.DisableRunAtStartup(Settings.RegistryStartupValueName);
                        Log.Info("Disabled Run at Startup.");
                    }

                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to create run at startup registry key!", $"{{ Value: '{Settings.RegistryStartupValueName}', Path: '{ExecutablePath}' }}", ex);
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the window should be minimized during startup.<br/>
        /// The window can be shown again later using the tray icon.
        /// </summary>
        public bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                NotifyPropertyChanging();
                _startMinimized = value;
                NotifyPropertyChanged();
            }
        }
        private bool _startMinimized;
        /// <summary>
        /// Gets or sets whether the program should check for updates.<br/>
        /// 
        /// </summary>
        public bool CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                NotifyPropertyChanging();
                _checkForUpdates = value;
                NotifyPropertyChanged();
            }
        }
        private bool _checkForUpdates;
        /// <summary>
        /// Gets or sets whether notifications are enabled or not.
        /// </summary>
        public bool NotificationEnabled
        {
            get => _notificationEnabled;
            set
            {
                NotifyPropertyChanging();
                _notificationEnabled = value;
                NotifyPropertyChanged();
            }
        }
        private bool _notificationEnabled;
        public int NotificationTimeout
        {
            get => _notificationTimeout;
            set
            {
                NotifyPropertyChanging();
                _notificationTimeout = value;
                NotifyPropertyChanged();
            }
        }
        private int _notificationTimeout;
        public ListNotificationDisplayTarget NotificationMode
        {
            get => _notificationMode;
            set
            {
                NotifyPropertyChanging();
                _notificationMode = value;
                NotifyPropertyChanged();
            }
        }
        private ListNotificationDisplayTarget _notificationMode = ListNotificationDisplayTarget.Sessions;
        #endregion Settings
        #endregion Properties

        #region EventHandlers
        protected virtual void Handle_AudioAPI_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null)
            {
                return;
            }

            if (e.PropertyName.Equals("Sessions"))
            { // reset autocomplete suggestions
                _targetAutoCompleteSource = null;
                NotifyPropertyChanged(nameof(TargetAutoCompleteSource));
            }
            else if (e.PropertyName.Equals("Devices"))
            {

            }

            NotifyPropertyChanged($"AudioAPI.{e.PropertyName}");
        }
        #endregion EventHandlers

        #region Methods
        /// <summary>Displays a message box prompting the user for confirmation, and if confirmation is given, resets all hotkeys to their default settings using <see cref="HotkeyManager.ResetHotkeys"/>.</summary>
        public void ResetHotkeySettings()
        {
            if (MessageBox.Show("Are you sure you want to reset your hotkeys to their default values?\n\nThis cannot be undone!", "Reset Hotkeys?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                HotkeyAPI.ResetHotkeys();

                Log.Info("Hotkey definitions were reset to default.");
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SaveSettings();
                    // Dispose of objects
                    AudioAPI.Dispose();
                    HotkeyAPI.Dispose();
                }

                disposedValue = true;
            }
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        ~VolumeControlSettings()
        {
            Dispose(disposing: true);
        }
        #endregion Methods
    }
}
