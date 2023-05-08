﻿using Newtonsoft.Json;
using Semver;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VolumeControl.Core.Enum;
using VolumeControl.Core.Helpers;
using VolumeControl.Core.Input;
using VolumeControl.Log;
using VolumeControl.WPF.Collections;

namespace VolumeControl.Core
{
    /// <summary>
    /// Contains the application configuration and logic to read from and write to JSON files.
    /// </summary>
    [JsonObject]
    public class Config : AppConfig.ConfigurationFile
    {
        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Config"/> instance.
        /// </summary>
        /// <remarks>The first time this is called, the <see cref="AppConfig.Configuration.Default"/> property is set to that instance; all subsequent calls do not update this property.</remarks>
        public Config() : base(_filePath) => this.ResumeAutoSave();
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Resumes automatic saving of the config to disk whenever a <see cref="PropertyChanged"/> event is triggered.
        /// </summary>
        public void ResumeAutoSave() => PropertyChanged += this.HandlePropertyChanged;
        /// <summary>
        /// Pauses automatic saving of the config to disk whenever a <see cref="PropertyChanged"/> event is triggered.
        /// </summary>
        public void PauseAutoSave() => PropertyChanged -= this.HandlePropertyChanged;
        #endregion Methods

        #region EventHandlers
        private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Log.Debug($"Property '{e.PropertyName}' was modified, saving {nameof(Config)}...");
            this.Save();
        }
        #endregion EventHandlers

        #region Statics
        private static LogWriter Log => FLog.Log;
        // Default filepath used for the config file:
        private const string _filePath = "VolumeControl.json";
        #endregion Statics

        #region Main

        /// <summary>
        /// Socket host address
        /// </summary>
        public string SocketHost { get; set; } = "https://localhost:5001/devices?CLH.client";

        /// <summary>
        /// Password for window shutdown operation.
        /// </summary>
        public string WindowsShutdownPassword { get; set; } = "password";

        /// <summary>
        /// Gets or sets the name of the current localization language.
        /// </summary>
        /// <remarks><b>Default: "English (US/CA)"</b></remarks>
        public string LanguageName { get; set; } = "English (US/CA)";
        /// <summary>
        /// Gets or sets whether the program should create the default translation files if they don't already exist.
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool CreateDefaultTranslationFiles { get; set; } = true;
        /// <summary>
        /// Gets or sets a list of additional directories to load localization packages from.
        /// </summary>
        /// <remarks><b>Default: {}</b></remarks>
        public ObservableImmutableList<string> CustomLocalizationDirectories { get; set; } = new();
        /// <summary>
        /// Gets or sets whether multiple distinct instances of Volume Control are allowed to run concurrently.<br/>
        /// In this case, <i>distinct</i> means that each instance is using a different config file.<br/><br/>
        /// This works by creating a MD5 hash of <see cref="AppConfig.ConfigurationFile.Location"/>, and appending it to the instance's mutex ID, seperated with a colon (:).<br/>
        /// The initialization process of acquiring a mutex lock is still performed, which will always prevent Volume Control from running alongside other instances using the same config as itself, regardless of this setting.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool AllowMultipleDistinctInstances { get; set; } = false;
        /// <summary>
        /// Gets or sets whether the application should run when Windows starts.<br/>
        /// Creates/deletes a registry value in <b>HKEY_CURRENT_USER => SOFTWARE\Microsoft\Windows\CurrentVersion\Run</b>
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool RunAtStartup { get; set; } = false;
        /// <summary>
        /// Gets or sets whether the window should be minimized during startup.<br/>
        /// The window can be shown again later using the tray icon.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool StartMinimized { get; set; } = false;
        /// <summary>
        /// Gets or sets whether the taskbar icon is visible when the window isn't minimized.
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool ShowInTaskbar { get; set; } = true;
        /// <summary>
        /// Gets or sets whether volume control should always appear on top of other windows when the window isn't minimized.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool AlwaysOnTop { get; set; } = false;
        /// <summary>
        /// Gets or sets whether confirmation is required to delete a hotkey.
        /// </summary>
        public bool DeleteHotkeyConfirmation { get; set; } = true;
        /// <summary>
        /// Gets or sets whether or not device/session icons are shown in the UI.
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool ShowIcons { get; set; } = true;
        /// <summary>
        /// List of directories that should be checked for addons in addition to the default one.
        /// </summary>
        /// <remarks><b>Default: {}</b></remarks>
        public ObservableImmutableList<string> CustomAddonDirectories { get; set; } = new();
        /// <summary>
        /// Gets or sets whether the main window allows transparency effects.<br/>
        /// <list type="bullet">
        /// <item>When <see langword="true"/>, window background transparency is allowed.</item>
        /// <item>When <see langword="false"/>, window background transparency isn't allowed;<br/>only controls <i>within</i> the window may use transparent background effects.<br/>(See <see cref="System.Windows.Controls.Control.Background"/>)</item>
        /// </list>
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool AllowTransparency { get; set; } = true;
        /// <summary>
        /// List of <see cref="Key"/>s that shouldn't be included as an option in the key selector.
        /// </summary>
        public static readonly List<Key> KeyBlacklist = new()
        {
            // Alt Modifier
            Key.LeftAlt,
            Key.RightAlt,
            // Ctrl Modifier:
            Key.LeftCtrl,
            Key.RightCtrl,
            // Shift Modifier:
            Key.LeftShift,
            Key.RightShift,
            // Super/Windows Modifier:
            Key.LWin,
            Key.RWin,
            // Duplicate keys & unused keys:
            Key.NoName,
            Key.System,
            Key.Prior,
            Key.Next,
            Key.Snapshot,
            Key.Print,
            Key.Pa1,
            // All 'Ime-', 'Oem-', & 'Dbe-' key entries:
            Key.ImeConvert,
            Key.ImeNonConvert,
            Key.ImeAccept,
            Key.ImeModeChange,
            Key.Oem1,
            Key.OemSemicolon,
            Key.OemPlus,
            Key.OemComma,
            Key.OemMinus,
            Key.OemPeriod,
            Key.Oem2,
            Key.OemQuestion,
            Key.Oem3,
            Key.OemTilde,
            Key.Oem4,
            Key.OemOpenBrackets,
            Key.Oem5,
            Key.OemPipe,
            Key.Oem6,
            Key.OemCloseBrackets,
            Key.Oem7,
            Key.OemQuotes,
            Key.Oem8,
            Key.Oem102,
            Key.OemBackslash,
            Key.ImeProcessed,
            Key.DbeAlphanumeric,
            Key.OemAttn,
            Key.DbeKatakana,
            Key.OemFinish,
            Key.DbeHiragana,
            Key.OemCopy,
            Key.DbeSbcsChar,
            Key.OemAuto,
            Key.DbeDbcsChar,
            Key.OemEnlw,
            Key.DbeRoman,
            Key.OemBackTab,
            Key.DbeNoRoman,
            Key.DbeEnterWordRegisterMode,
            Key.DbeEnterImeConfigureMode,
            Key.DbeFlushString,
            Key.DbeCodeInput,
            Key.DbeNoCodeInput,
            Key.DbeDetermineString,
            Key.DbeEnterDialogConversionMode,
            Key.OemClear,
        };
        #endregion Main

        #region Notifications
        /// <summary>
        /// The brush to use for the background of the list notification when locked.
        /// </summary>
        [JsonIgnore]
        public static readonly Brush NotificationLockedBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromRgb(0xA3, 0x28, 0x28), 0.2),
            new GradientStop(Color.FromRgb(0xA8, 0x14, 0x14), 0.8)
        }, new Point(0, 0), new Point(1, 1));
        /// <summary>
        /// The brush to use for the background of the list notification when unlocked.
        /// </summary>
        [JsonIgnore]
        public static readonly Brush NotificationUnlockedBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromRgb(0x49, 0x6D, 0x49), 0.2),
            new GradientStop(Color.FromRgb(0x40, 0x70, 0x40), 0.8)
        }, new Point(0, 0), new Point(1, 1));
        /// <summary>
        /// The default background brush used for the ListNotification
        /// </summary>
        [JsonIgnore]
        public static readonly Brush NotificationDefaultBrush = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
        /// <summary>
        /// Gets or sets whether notifications are enabled or not.
        /// </summary>
        public bool NotificationsEnabled { get; set; } = true;
        /// <summary>
        /// Gets or sets whether or not the list notification window appears for volume change events.
        /// </summary>
        public bool NotificationsOnVolumeChange { get; set; } = true;
        /// <summary>
        /// The amount of time, in milliseconds, that the notification window stays visible for before disappearing.
        /// </summary>
        public int NotificationTimeoutMs { get; set; } = 3000;
        /// <summary>
        /// Gets or sets whether the list notification timeout is enabled, which causes the list notification to disappear after <see cref="NotificationTimeoutMs"/> milliseconds.
        /// </summary>
        public bool NotificationTimeoutEnabled { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the notification can be dragged without holding down the ALT key.
        /// </summary>
        public bool NotificationMoveRequiresAlt { get; set; } = true;
        /// <summary>
        /// Gets or sets the location of the notification window
        /// </summary>
        public Point? NotificationPosition { get; set; }
        /// <summary>
        /// Gets or sets whether <see cref="NotificationPosition"/> is used or not.
        /// </summary>
        public bool NotificationSavePos { get; set; } = true;
        /// <summary>
        /// Gets or sets whether controls from <see cref="Interfaces.IListDisplayable.DisplayControls"/> are shown or not.
        /// </summary>
        //public bool NotificationShowsCustomControls { get; set; } = true;
        /// <summary>
        /// Gets or sets the corner from which ListNotification size transform operations are rooted.
        /// </summary>
        public ScreenCorner NotificationPositionOriginCorner { get; set; } = ScreenCorner.BottomLeft;
        /// <summary>
        /// Gets or sets whether the notification window slowly fades in instead of appearing instantly.
        /// </summary>
        public bool NotificationDoFadeIn { get; set; } = true;
        /// <summary>
        /// Gets or sets the duration of the notification fade-in animation.
        /// </summary>
        public Duration NotificationFadeInDuration { get; set; } = new(TimeSpan.FromMilliseconds(300));
        /// <summary>
        /// Gets or sets whether the notification window slowly fades out instead of disappearing instantly.
        /// </summary>
        public bool NotificationDoFadeOut { get; set; } = true;
        /// <summary>
        /// Gets or sets the duration of the notification fade-in animation.
        /// </summary>
        public Duration NotificationFadeOutDuration { get; set; } = new(TimeSpan.FromMilliseconds(500));
        /// <summary>
        /// Gets or sets the name of the currently-selected list notification display target.
        /// </summary>
        public string NotificationDisplayTarget { get; set; } = "Audio Sessions";
        #endregion Notifications

        #region Updates
        /// <summary>
        /// Gets or sets whether the program should check for updates on startup.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;
        /// <summary>
        /// Gets or sets whether a message box prompt is shown informing the user of an available update.
        /// </summary>
        /// <remarks>When this is disabled, updates are notified through the caption bar.</remarks>
        public bool ShowUpdatePrompt { get; set; } = true;
        #endregion Updates

        #region Hotkeys
        /// <summary>
        /// The current hotkey configuration, or <see langword="null"/> if the default configuration should be used instead.
        /// </summary>
        public BindableHotkeyJsonWrapper[] Hotkeys { get; set; } = null!;
        /// <summary>
        /// Default hotkey configuration.
        /// </summary>
        [JsonIgnore]
        public static readonly BindableHotkeyJsonWrapper[] Hotkeys_Default = new BindableHotkeyJsonWrapper[]
        {
            new ()
            {
                Name = "Volume Up",
                Key = Key.VolumeUp,
                Modifier = Modifier.None,
                ActionIdentifier = "Session:Volume Up",
            },
            new()
            {
                Name = "Volume Down",
                Key = Key.VolumeDown,
                Modifier = Modifier.None,
                ActionIdentifier = "Session:Volume Down",
            },
            new()
            {
                Name = "Toggle Mute",
                Key = Key.VolumeMute,
                Modifier = Modifier.None,
                ActionIdentifier = "Session:Toggle Mute",
            },
            new()
            {
                Name = "Next Session",
                Key = Key.E,
                Modifier = Modifier.Alt | Modifier.Shift | Modifier.Ctrl,
                ActionIdentifier = "Session:Select Next",
            },
            new()
            {
                Name = "Previous Session",
                Key = Key.Q,
                Modifier = Modifier.Alt | Modifier.Shift | Modifier.Ctrl,
                ActionIdentifier = "Session:Select Previous",
            },
            new()
            {
                Name = "Un/Lock Session",
                Key = Key.S,
                Modifier = Modifier.Alt | Modifier.Shift | Modifier.Ctrl,
                ActionIdentifier = "Session:Toggle Lock",
            }
        };
        #endregion Hotkeys

        #region Audio
        /// <summary>
        /// Gets or sets the last target session.
        /// </summary>
        public TargetInfo Target { get; set; } = new() { ProcessIdentifier = string.Empty, SessionInstanceIdentifier = string.Empty };
        /// <summary>
        /// Gets or sets whether the target session is locked.
        /// </summary>
        public bool LockTargetSession { get; set; } = false;
        /// <summary>
        /// Gets or sets the amount of volume added or subtracted from the current volume level when volume change hotkeys are pressed.
        /// </summary>
        public int VolumeStepSize { get; set; } = 2;
        /// <summary>
        /// List of enabled audio device IDs.
        /// </summary>
        public ObservableImmutableList<string> EnabledDevices { get; set; } = new() { };
        /// <summary>
        /// Whether the default audio device should be enabled or not, regardless of the device IDs in <see cref="EnabledDevices"/>.
        /// </summary>
        public bool EnableDefaultDevice { get; set; } = true;
        /// <summary>
        /// Gets or sets the interval (in milliseconds) between updating the audio peak meters.
        /// </summary>
        /// <remarks><b>Default: 500ms</b></remarks>
        public int PeakMeterUpdateIntervalMs { get; set; } = 100;
        /// <summary>
        /// Gets or sets whether or not peak meters are shown in the mixer.
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool ShowPeakMeters { get; set; } = true;
        /// <summary>
        /// The minimum boundary shown on peak meters.
        /// </summary>
        /// <remarks><b>0.0</b></remarks>
        public const double PeakMeterMinValue = 0.0;
        /// <summary>
        /// The maximum boundary shown on peak meters.
        /// </summary>
        /// <remarks><b>1.0</b></remarks>
        public const double PeakMeterMaxValue = 1.0;
        /// <summary>
        /// Gets or sets whether volume &amp; mute controls are visible in the Audio Devices list.
        /// </summary>
        /// <remarks><b>Default: <see langword="false"/></b></remarks>
        public bool EnableDeviceControl { get; set; } = false;
        #endregion Audio

        #region Log
        /// <summary>
        /// Gets or sets whether the log is enabled or not.<br/>
        /// See <see cref="Log.SettingsInterface.EnableLogging"/>
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool EnableLogging { get; set; } = true;
        /// <summary>
        /// Gets or sets the location of the log file.<br/>
        /// See <see cref="Log.SettingsInterface.LogPath"/>
        /// </summary>
        /// <remarks><b>Default: "VolumeControl.log"</b></remarks>
        public string LogPath { get; set; } = "VolumeControl.log";
        /// <summary>
        /// Gets or sets the <see cref="Log.Enum.EventType"/> filter used for messages.<br/>
        /// See <see cref="Log.SettingsInterface.LogFilter"/>
        /// </summary>
        /// <remarks><b>Default: <see cref="Log.Enum.EventType.ALL_EXCEPT_DEBUG"/></b></remarks>
        public VolumeControl.Log.Enum.EventType LogFilter { get; set; } = VolumeControl.Log.Enum.EventType.ALL_EXCEPT_DEBUG;
        /// <summary>
        /// Gets or sets whether the log is cleared when the program starts.<br/>
        /// See <see cref="Log.SettingsInterface.LogClearOnInitialize"/>
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool LogClearOnInitialize { get; set; } = true;
        /// <summary>
        /// Gets or sets the format string used for timestamps in the log.<br/>
        /// See <see cref="Log.SettingsInterface.LogTimestampFormat"/>
        /// </summary>
        /// <remarks><b>Default: "HH:mm:ss:fff"</b></remarks>
        public string LogTimestampFormat { get; set; } = "HH:mm:ss:fff";
        /// <summary>
        /// 
        /// See <see cref="Log.SettingsInterface.LogEnableStackTrace"/>
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool LogEnableStackTrace { get; set; } = true;
        /// <summary>
        /// 
        /// See <see cref="Log.SettingsInterface.LogEnableStackTraceLineCount"/>
        /// </summary>
        /// <remarks><b>Default: <see langword="true"/></b></remarks>
        public bool LogEnableStackTraceLineCount { get; set; } = true;
        #endregion Log

        #region Misc
        /// <summary>
        /// Gets or sets the version number identifier of the last Volume Control instance to access this config file.
        /// </summary>
        /// <remarks><b>Default: 0.0.0</b></remarks>
        public SemVersion __VERSION__ { get; set; } = new(0);
        #endregion Misc
    }
}
