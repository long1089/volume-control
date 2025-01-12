﻿using VolumeControl.Audio;
using VolumeControl.Core;
using VolumeControl.Log;

namespace VolumeControl.SDK
{
    /// <summary>
    /// Exposes the primary interaction point with the Volume Control API to addons.<br/>
    /// See the <see cref="VCAPI.Default"/> property for more information.<br/>
    /// </summary>
    /// <remarks>
    /// Have an idea for extending the Volume Control API? We'd love to hear it! Let us know with a <see href="https://github.com/radj307/volume-control/issues/new?assignees=&amp;labels=&amp;template=feature-request.md&amp;title=%5BREQUEST%5D">Feature Request</see>, or by submitting a <see href="https://github.com/radj307/volume-control/compare">Pull Request</see>.
    /// </remarks>
    public class VCAPI
    {
        internal VCAPI(AudioAPI audioAPI, HotkeyManager hkManager, IntPtr MainHWnd, Config settings)
        {
            this.AudioAPI = audioAPI;
            this.HotkeyManager = hkManager;
            this.MainWindowHWnd = MainHWnd;
            this.Settings = settings;
            this.ListDisplayTargets = new();
        }

        #region Statics
        /// <summary>
        /// This is the default instance of VCAPI.<br/>You must use this to access the API.
        /// </summary>
        public static VCAPI Default { get; internal set; } = null!;
        /// <summary>
        /// This is the default log instance.
        /// </summary>
        public static LogWriter Log => FLog.Log;
        #endregion Statics

        #region Properties
        /// <summary>
        /// This is the global <see cref="Audio.AudioAPI"/>.
        /// </summary>
        public AudioAPI AudioAPI { get; }
        /// <summary>
        /// This is the global <see cref="Core.HotkeyManager"/>.
        /// </summary>
        public HotkeyManager HotkeyManager { get; }
        /// <summary>
        /// This is the handle of the main window, for use with Windows API methods in <see cref="User32"/> or elsewhere.
        /// </summary>
        public IntPtr MainWindowHWnd { get; }
        /// <summary>
        /// This is the object that contains many of the runtime application settings.<br/>These are saved to the user configuration when the application shuts down.
        /// </summary>
        public Config Settings { get; }
        /// <summary>
        /// The list of <see cref="ListDisplayTarget"/> instances currently 
        /// </summary>
        public List<ListDisplayTarget> ListDisplayTargets { get; }
        #endregion Properties
    }
}