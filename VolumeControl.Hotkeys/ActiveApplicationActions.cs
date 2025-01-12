﻿using System.Runtime.InteropServices;
using VolumeControl.Audio;
using VolumeControl.Audio.Interfaces;
using VolumeControl.Core.Attributes;
using VolumeControl.Core.Input.Actions;
using VolumeControl.SDK;

namespace VolumeControl.Hotkeys
{
    /// <summary>
    /// Defines actions that affect the current foreground application.
    /// </summary>
    [HotkeyActionGroup("Active Application", GroupColor = "#9F87FF")]
    public class ActiveApplicationActions
    {
        #region Properties
        private static AudioAPI AudioAPI => VCAPI.Default.AudioAPI;
        #endregion Properties

        #region Functions
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);
        /// <summary>
        /// Gets the associated <see cref="ISession"/> of the current foreground window.
        /// </summary>
        /// <returns>The <see cref="ISession"/> associated with the current foreground application, if one was found; otherwise <see langword="null"/>.</returns>
        private static ISession? GetActiveSession()
        {
            var hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return null;

            if (GetWindowThreadProcessId(hwnd, out int pid) == 0)
                return null;

            if (AudioAPI.FindSessionWithID(pid) is ISession session)
                return session; //< found with process ID

            return AudioAPI.FindSessionWithName(System.Diagnostics.Process.GetProcessById(pid).ProcessName);
        }
        #endregion Functions

        #region Methods
        [HotkeyAction(Description = "Increases the volume of the current foreground application.")]
        public void VolumeUp(object? sender, HotkeyActionPressedEventArgs e)
        {
            if (GetActiveSession() is ISession session)
            {
                session.Volume += AudioAPI.VolumeStepSize;
            }
        }
        [HotkeyAction(Description = "Decreases the volume of the current foreground application.")]
        public void VolumeDown(object? sender, HotkeyActionPressedEventArgs e)
        {
            if (GetActiveSession() is ISession session)
            {
                session.Volume -= AudioAPI.VolumeStepSize;
            }
        }
        [HotkeyAction(Description = "Mutes the current foreground application.")]
        public void Mute(object? sender, HotkeyActionPressedEventArgs e)
        {
            if (GetActiveSession() is ISession session)
            {
                session.Muted = true;
            }
        }
        [HotkeyAction(Description = "Unmutes the current foreground application.")]
        public void Unmute(object? sender, HotkeyActionPressedEventArgs e)
        {
            if (GetActiveSession() is ISession session)
            {
                session.Muted = false;
            }
        }
        [HotkeyAction(Description = "(Un)Mutes the current foreground application.")]
        public void ToggleMute(object? sender, HotkeyActionPressedEventArgs e)
        {
            if (GetActiveSession() is ISession session)
            {
                session.Muted = !session.Muted;
            }
        }
        #endregion Methods
    }
}
