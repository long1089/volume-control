﻿using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using PropertyChanged;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
using VolumeControl.Audio.Events;
using VolumeControl.Audio.Interfaces;
using VolumeControl.Core.Interfaces;
using VolumeControl.Log;
using VolumeControl.TypeExtensions;
using VolumeControl.WPF;
using VolumeControl.WPF.Collections;

namespace VolumeControl.Audio
{
    /// <summary>
    /// An audio device.
    /// </summary>
    /// <remarks>
    /// <b>This object cannot be constructed outside of the <see cref="VolumeControl"/>.<see cref="VolumeControl.Audio"/> assembly!</b><br/>
    /// </remarks>
    public sealed class AudioDevice : IVolumeControl, IListDisplayable, IDevice, IDisposable, IEquatable<AudioDevice>, IEquatable<IDevice>, IList, ICollection, IEnumerable, IList<AudioSession>, IImmutableList<AudioSession>, ICollection<AudioSession>, IEnumerable<AudioSession>, IReadOnlyList<AudioSession>, IReadOnlyCollection<AudioSession>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors
        /// <inheritdoc cref="AudioDevice"/>
        /// <param name="device">An enumerated <see cref="MMDevice"/> instance from NAudio.</param>
        internal AudioDevice(MMDevice device)
        {
            this.MMDevice = device;
            this.DeviceID = this.MMDevice.ID;
            _name = this.GetDeviceName();

            if (this.SessionManager != null)
            {
                this.SessionManager.OnSessionCreated += this.HandleSessionCreated;
                this.ReloadSessions();
            }

            this.EndpointVolumeObject.OnVolumeNotification += (e) =>
            {
                this.ForwardVolumeChanged(e);
                this.NotifyPropertyChanged(nameof(this.Volume));
                this.NotifyPropertyChanged(nameof(this.Muted));
            };
        }
        #endregion Constructors

        #region Fields
        private readonly object endpointVolumeObjectLock = new();
        #endregion Fields

        #region Properties
        /// <inheritdoc/>
        public AudioMeterInformation AudioMeterInformation => this.MMDevice.AudioMeterInformation;
        /// <inheritdoc/>
        public float PeakMeterValue => this.AudioMeterInformation.MasterPeakValue;
        private static LogWriter Log => FLog.Log;
        internal MMDevice MMDevice { get; }
        /// <summary>Whether this device is enabled by the user or not.</summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                this.NotifyPropertyChanged();
                this.NotifyEnabledChanged();
            }
        }
        private bool _enabled = false;
        /// <inheritdoc/>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string _name;
        /// <summary>The name of the audio interface; for example 'USB Audio Codec'</summary>
        public string InterfaceName => this.DeviceFriendlyName;
        /// <inheritdoc/>
        public string DeviceID { get; }
        /// <summary>The instance ID.</summary>
        public string InstanceID => this.MMDevice.InstanceId;
        /// <summary>This is the self-reported location of this device's icon on the system and may or may not actually be set.</summary>
        public string IconPath => this.MMDevice.IconPath;
        private IconPair? _icons = null;
        /// <summary>
        /// Gets or sets the icons associated with this <see cref="AudioDevice"/>.
        /// </summary>
        public IconPair Icons
        {
            get => _icons ??= this.GetIcons();
            set => _icons = value;
        }
        /// <summary>
        /// Gets the icon associated with this <see cref="AudioDevice"/>.<br/>
        /// See <see cref="Icons"/>
        /// </summary>
        /// <remarks><see cref="IconPair.GetBestFitIcon(bool)"/>. Prioritizes small icons.</remarks>
        public ImageSource? Icon => this.Icons.GetBestFitIcon(false);
        /// <summary>
        /// Gets the friendly name of the endpoint adapter, excluding the name of the device.<br/>Example: "Speakers (XYZ Audio Adapter)"
        /// </summary>
        /// <remarks>For more information on this property, see MSDN: <see href="https://docs.microsoft.com/en-us/windows/win32/coreaudio/pkey-device-friendlyname">PKEY_Device_FriendlyName</see></remarks>
        public string DeviceFriendlyName => this.MMDevice.DeviceFriendlyName;
        /// <summary>
        /// Gets the friendly name of the endpoint device, including the name of the adapter.<br/>Example: "Speakers (XYZ Audio Adapter)"
        /// </summary>
        /// <remarks>For more information on this property, see MSDN: <see href="https://docs.microsoft.com/en-us/windows/win32/coreaudio/pkey-device-friendlyname">PKEY_Device_FriendlyName</see></remarks>
        public string FriendlyName => this.MMDevice.FriendlyName;
        /// <summary>
        /// Gets the underlying <see cref="PropertyStore"/> object for this endpoint.
        /// </summary>
        /// <remarks>This object is from NAudio.<br/>If you're writing an addon, install the 'NAudio' nuget package to be able to use this.</remarks>
        public PropertyStore Properties => this.MMDevice.Properties;
        /// <summary>
        /// Gets the current <see cref="DeviceState"/> of this endpoint.<br/>For more information, see the MSDN page: <see href="https://docs.microsoft.com/en-us/windows/win32/coreaudio/device-state-xxx-constants">DEVICE_STATE_XXX Constants</see>
        /// </summary>
        /// <remarks>This object is from NAudio.<br/>If you're writing an addon, install the 'NAudio' nuget package to be able to use this.</remarks>
        public DeviceState State => this.MMDevice.State;
        /// <summary>
        /// Gets the <see cref="AudioSessionManager"/> object from the underlying <see cref="MMDevice"/>.
        /// </summary>
        /// <remarks>This object is from NAudio.<br/>If you're writing an addon, install the 'NAudio' nuget package to be able to use this.</remarks>
        public AudioSessionManager? SessionManager => this.MMDevice.State.Equals(DeviceState.Active) ? this.MMDevice.AudioSessionManager : null;
        /// <summary>
        /// Gets the <see cref="AudioEndpointVolume"/> object from the underlying <see cref="MMDevice"/>.
        /// </summary>
        /// <remarks>This object is from NAudio.<br/>If you're writing an addon, install the 'NAudio' nuget package to be able to use this.</remarks>
        public AudioEndpointVolume EndpointVolumeObject
        {
            get
            {
                lock (endpointVolumeObjectLock) //< this is needed because multiple simultaneous requests from different threads will throw an E_NOINTERFACE error. (This is uncommon but happened 3+ times during development where multiple sliders for the same device were present.) This is likely due to NAudio converting COM objects behind the scenes.
                {
                    return this.MMDevice.AudioEndpointVolume;
                }
            }
        }
        /// <inheritdoc/>
        public float NativeVolume
        {
            get => this.EndpointVolumeObject.MasterVolumeLevelScalar;
            set
            {
                this.EndpointVolumeObject.MasterVolumeLevelScalar = value;
                this.NotifyPropertyChanged();
            }
        }
        /// <inheritdoc/>
        public int Volume
        {
            get => Convert.ToInt32(this.NativeVolume * 100f);
            set
            {
                this.NativeVolume = (float)(Convert.ToDouble(MathExt.Clamp(value, 0, 100)) / 100.0);
                this.NotifyPropertyChanged();
            }
        }
        /// <inheritdoc/>
        public bool Muted
        {
            get => this.EndpointVolumeObject.Mute;
            set
            {
                this.EndpointVolumeObject.Mute = value;
                this.NotifyPropertyChanged();
            }
        }
        /// <inheritdoc/>
        public string DisplayText
        {
            get => this.DeviceFriendlyName;
            set { }
        }
        private Control[]? _displayControls;
        /// <inheritdoc/>
        public Control[]? DisplayControls => _displayControls ??= IMeteredVolumeControl.MakeListDisplayableControlTemplate(this);
        /// <inheritdoc/>
        public ImageSource? DisplayIcon => this.Icon;

        /// <summary>
        /// The sessions that are playing on this device.
        /// </summary>
        [SuppressPropertyChangedWarnings]
        public ObservableImmutableList<AudioSession> Sessions { get; } = new();
        #region InterfaceProperties
        /// <inheritdoc/>
        public bool IsFixedSize => ((IList)this.Sessions).IsFixedSize;
        /// <inheritdoc/>
        public bool IsReadOnly => ((IList)this.Sessions).IsReadOnly;
        /// <inheritdoc/>
        public int Count => ((ICollection)this.Sessions).Count;
        /// <inheritdoc/>
        public bool IsSynchronized => ((ICollection)this.Sessions).IsSynchronized;
        /// <inheritdoc/>
        public object SyncRoot => ((ICollection)this.Sessions).SyncRoot;
        /// <inheritdoc/>
        [SuppressPropertyChangedWarnings]
        AudioSession IReadOnlyList<AudioSession>.this[int index] => ((IReadOnlyList<AudioSession>)this.Sessions)[index];
        /// <inheritdoc/>
        [SuppressPropertyChangedWarnings]
        public AudioSession this[int index] { get => ((IList<AudioSession>)this.Sessions)[index]; set => ((IList<AudioSession>)this.Sessions)[index] = value; }
        /// <inheritdoc/>
        [SuppressPropertyChangedWarnings]
        object? IList.this[int index] { get => ((IList)this.Sessions)[index]; set => ((IList)this.Sessions)[index] = value; }
        #endregion InterfaceProperties
        #endregion Properties

        #region Events
        /// <summary>Triggered when an audio session is removed from this device.</summary>
        public event EventHandler<AudioSession>? SessionRemoved;
        private void NotifySessionRemoved(AudioSession session) => SessionRemoved?.Invoke(this, session);
        /// <summary>Triggered when an audio session is added to this device.</summary>
        public event EventHandler<AudioSession>? SessionCreated;
        private void NotifySessionCreated(AudioSession session) => SessionCreated?.Invoke(this, session);
        /// <summary>Triggered when the value of the <see cref="Enabled"/> property was changed.</summary>
        public event EventHandler<bool>? EnabledChanged;
        private void NotifyEnabledChanged() => EnabledChanged?.Invoke(this, this.Enabled);
        /// <summary>Triggered when any property in the <see cref="Properties"/> container was changed.</summary>
        public event EventHandler<PropertyKey>? PropertyStoreChanged;
        internal void ForwardPropertyStoreChanged(object? _, PropertyKey propertyKey) => PropertyStoreChanged?.Invoke(this, propertyKey);

        /// <summary>Audio device was removed.</summary>
        public event EventHandler? Removed;
        internal void ForwardRemoved(object? _, EventArgs e) => Removed?.Invoke(this, e);

        /// <summary>Triggered when the <see cref="State"/> property is changed.</summary>
        /// <remarks>This event is routed from the windows API.</remarks>
        internal event EventHandler<DeviceState>? StateChanged;
        internal void ForwardStateChanged(object? _, DeviceState e) => StateChanged?.Invoke(this, e);

        /// <summary>
        /// Triggered when the endpoint volume changes from any source.
        /// </summary>
        public event VolumeChangedEventHandler? VolumeChanged;
        private void ForwardVolumeChanged(AudioVolumeNotificationData data) => VolumeChanged?.Invoke(this, new(data));
        /// <summary>Triggered when the <see cref="Sessions"/> collection is modified.</summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => ((INotifyCollectionChanged)this.Sessions).CollectionChanged += value;
            remove => ((INotifyCollectionChanged)this.Sessions).CollectionChanged -= value;
        }
        /// <summary>Triggered when a property is set.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new(propertyName));
        #endregion Events

        #region SessionEventHandlers
        /// <summary>Handles <see cref="AudioSessionManager.OnSessionCreated"/> events, and adds them to the device's session list.</summary>
        private void HandleSessionCreated(object? sender, IAudioSessionControl controller)
        {
            var session = new AudioSession(new AudioSessionControl(controller));
            // Add session to the list
            _ = this.Sessions.Add(session);
            this.NotifySessionCreated(session);

            // Bind events to handlers:
            session.StateChanged += this.HandleSessionStateChanged;

            Log.Debug($"{session.ProcessIdentifier} created an audio session on device '{this.Name}'");
        }
        /// <summary>Handles <see cref="AudioSession.StateChanged"/> events from sessions within the <see cref="Sessions"/> list.</summary>
        private void HandleSessionStateChanged(object? sender, AudioSessionState state)
        {
            if (sender is AudioSession session)
            {
                switch (state)
                {
                case AudioSessionState.AudioSessionStateExpired: // triggered when a session is closing
                case AudioSessionState.AudioSessionStateInactive: // triggered when a session is deactivated
                    if (!session.IsRunning)
                    {
                        // unbind events because why not
                        session.StateChanged -= this.HandleSessionStateChanged;
                        session.NotifyExited();
                        // Remove the session from the list & dispose of it
                        _ = this.Sessions.Remove(session);
                        this.NotifySessionRemoved(session);
                        Log.Debug($"{session.ProcessIdentifier} exited.");
                        session.Dispose();
                        session = null!;
                        return;
                    }
                    break;
                case AudioSessionState.AudioSessionStateActive: // triggered when a session is activated
                default: break;
                }
                Log.Debug($"{session.ProcessIdentifier} state changed to {state:G}");
            }
            else
            {
                throw new InvalidOperationException($"{nameof(HandleSessionStateChanged)} was called with illegal type '{sender?.GetType().FullName}' for parameter '{nameof(sender)}'! Expected an object of type {typeof(AudioSession).FullName}");
            }
        }
        #endregion SessionEventHandlers

        #region Methods
        /// <summary>
        /// Gets the actual device name from the given <paramref name="deviceFriendlyName"/> by using regex.
        /// </summary>
        /// <param name="deviceFriendlyName"><see cref="MMDevice.DeviceFriendlyName"/></param>
        /// <returns>The name of the device.</returns>
        public static string GetDeviceNameFromDeviceFriendlyName(string deviceFriendlyName) => Regex.Replace(deviceFriendlyName, $"\\(\\s*?{deviceFriendlyName}\\s*?\\)", "", RegexOptions.Compiled).Trim();
        /// <summary>Gets the device name without the interface name.</summary>
        /// <returns><see cref="string"/></returns>
        public string GetDeviceName() => GetDeviceNameFromDeviceFriendlyName(this.DeviceFriendlyName);
        #region Sessions
        /// <summary>Clears the <see cref="Sessions"/> list, disposing of all items, and reloads all sessions from the <see cref="SessionManager"/>.</summary>
        /// <remarks>This should only be used when initializing a new device, or if an error occurs.<br/>If this method is called when the <see cref="State"/> property isn't set to <see cref="DeviceState.Active"/>, the session list is cleared without reloading.<br/>This is because inactive devices do not have a valid <see cref="SessionManager"/> object.</remarks>
        internal void ReloadSessions()
        {
            this.Sessions.ForEach(s => s.Dispose());
            _ = this.Sessions.Clear();

            if (this.SessionManager == null)
                return;

            this.SessionManager.RefreshSessions();
            SessionCollection? sessions = this.SessionManager.Sessions;

            for (int i = 0; i < sessions.Count; ++i)
            {
                var s = new AudioSession(sessions[i]);
                s.StateChanged += this.HandleSessionStateChanged;
                if (s.IsRunning)
                    _ = this.Sessions.Add(s);
            }
        }
        /// <summary>
        /// Gets the list of audio sessions currently using this device.
        /// </summary>
        /// <returns>A list of new <see cref="AudioSession"/> instances.</returns>
        internal static List<AudioSession> GetAudioSessions(AudioSessionManager manager)
        {
            manager.RefreshSessions();
            SessionCollection? sessions = manager.Sessions;
            List<AudioSession> l = new();
            for (int i = 0; i < sessions.Count; ++i)
            {
                var s = new AudioSession(sessions[i]);
                if (s.IsRunning)
                    l.Add(s);
            }
            return l;
        }
        /// <summary>Finds all sessions with the given <paramref name="predicate"/>.</summary>
        /// <param name="predicate">A predicate function that accepts <see cref="AudioSession"/>.</param>
        /// <returns>An array of matching <see cref="AudioSession"/> instances.</returns>
        public AudioSession[] FindAll(Predicate<AudioSession> predicate)
        {
            List<AudioSession> l = new();
            foreach (AudioSession? session in this.Sessions)
            {
                if (predicate(session))
                    l.Add(session);
            }

            return l.ToArray();
        }
        #endregion Sessions
        #region Other
        /// <inheritdoc cref="IconGetter.GetIcons(string)"/>
        public IconPair GetIcons()
        {
            try
            {
                return IconGetter.GetIcons(this.IconPath);
            }
            catch (Exception ex)
            {
                Log.Warning($"{nameof(AudioDevice)}.{nameof(GetIcons)} failed due to an exception!", ex);
            }
            return new();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            this.MMDevice.Dispose();
            _icons = null;
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        public bool Equals(AudioDevice? other) => other is not null && other.DeviceID.Equals(this.DeviceID, StringComparison.Ordinal);
        /// <inheritdoc/>
        public bool Equals(IDevice? other) => other is not null && other.DeviceID.Equals(this.DeviceID, StringComparison.Ordinal);
        /// <inheritdoc/>
        public override bool Equals(object? obj) => this.Equals(obj as AudioDevice);
        /// <inheritdoc/>
        public override int GetHashCode() => this.DeviceID.GetHashCode();
        #endregion Other
        #region InterfaceMethods
        /// <inheritdoc/>
        public int Add(object? value) => ((IList)this.Sessions).Add(value);
        /// <inheritdoc/>
        public void Clear() => ((IList)this.Sessions).Clear();
        /// <inheritdoc/>
        public bool Contains(object? value) => ((IList)this.Sessions).Contains(value);
        /// <inheritdoc/>
        public int IndexOf(object? value) => ((IList)this.Sessions).IndexOf(value);
        /// <inheritdoc/>
        public void Insert(int index, object? value) => ((IList)this.Sessions).Insert(index, value);
        /// <inheritdoc/>
        public void Remove(object? value) => ((IList)this.Sessions).Remove(value);
        /// <inheritdoc/>
        public void RemoveAt(int index) => ((IList)this.Sessions).RemoveAt(index);
        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => ((ICollection)this.Sessions).CopyTo(array, index);
        /// <inheritdoc/>
        public IEnumerator GetEnumerator() => ((IEnumerable)this.Sessions).GetEnumerator();
        /// <inheritdoc/>
        public int IndexOf(AudioSession item) => ((IList<AudioSession>)this.Sessions).IndexOf(item);
        /// <inheritdoc/>
        public void Insert(int index, AudioSession item) => ((IList<AudioSession>)this.Sessions).Insert(index, item);
        /// <inheritdoc/>
        public void Add(AudioSession item) => ((ICollection<AudioSession>)this.Sessions).Add(item);
        /// <inheritdoc/>
        public bool Contains(AudioSession item) => ((ICollection<AudioSession>)this.Sessions).Contains(item);
        /// <inheritdoc/>
        public void CopyTo(AudioSession[] array, int arrayIndex) => ((ICollection<AudioSession>)this.Sessions).CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public bool Remove(AudioSession item) => ((ICollection<AudioSession>)this.Sessions).Remove(item);
        /// <inheritdoc/>
        IEnumerator<AudioSession> IEnumerable<AudioSession>.GetEnumerator() => ((IEnumerable<AudioSession>)this.Sessions).GetEnumerator();
        /// <inheritdoc/>
        IImmutableList<AudioSession> IImmutableList<AudioSession>.Add(AudioSession value) => ((IImmutableList<AudioSession>)this.Sessions).Add(value);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> AddRange(IEnumerable<AudioSession> items) => ((IImmutableList<AudioSession>)this.Sessions).AddRange(items);
        /// <inheritdoc/>
        IImmutableList<AudioSession> IImmutableList<AudioSession>.Clear() => ((IImmutableList<AudioSession>)this.Sessions).Clear();
        /// <inheritdoc/>
        public int IndexOf(AudioSession item, int index, int count, IEqualityComparer<AudioSession>? equalityComparer) => ((IImmutableList<AudioSession>)this.Sessions).IndexOf(item, index, count, equalityComparer);
        /// <inheritdoc/>
        IImmutableList<AudioSession> IImmutableList<AudioSession>.Insert(int index, AudioSession element) => ((IImmutableList<AudioSession>)this.Sessions).Insert(index, element);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> InsertRange(int index, IEnumerable<AudioSession> items) => ((IImmutableList<AudioSession>)this.Sessions).InsertRange(index, items);
        /// <inheritdoc/>
        public int LastIndexOf(AudioSession item, int index, int count, IEqualityComparer<AudioSession>? equalityComparer) => ((IImmutableList<AudioSession>)this.Sessions).LastIndexOf(item, index, count, equalityComparer);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> Remove(AudioSession value, IEqualityComparer<AudioSession>? equalityComparer) => ((IImmutableList<AudioSession>)this.Sessions).Remove(value, equalityComparer);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> RemoveAll(Predicate<AudioSession> match) => ((IImmutableList<AudioSession>)this.Sessions).RemoveAll(match);
        /// <inheritdoc/>
        IImmutableList<AudioSession> IImmutableList<AudioSession>.RemoveAt(int index) => ((IImmutableList<AudioSession>)this.Sessions).RemoveAt(index);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> RemoveRange(IEnumerable<AudioSession> items, IEqualityComparer<AudioSession>? equalityComparer) => ((IImmutableList<AudioSession>)this.Sessions).RemoveRange(items, equalityComparer);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> RemoveRange(int index, int count) => ((IImmutableList<AudioSession>)this.Sessions).RemoveRange(index, count);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> Replace(AudioSession oldValue, AudioSession newValue, IEqualityComparer<AudioSession>? equalityComparer) => ((IImmutableList<AudioSession>)this.Sessions).Replace(oldValue, newValue, equalityComparer);
        /// <inheritdoc/>
        public IImmutableList<AudioSession> SetItem(int index, AudioSession value) => ((IImmutableList<AudioSession>)this.Sessions).SetItem(index, value);
        /// <inheritdoc/>
        public float IncreaseVolume(float amount) => NativeVolume += amount;
        /// <inheritdoc/>
        public int IncreaseVolume(int amount) => Volume += amount;
        /// <inheritdoc/>
        public float DecreaseVolume(float amount) => NativeVolume -= amount;
        /// <inheritdoc/>
        public int DecreaseVolume(int amount) => Volume -= amount;
        #endregion InterfaceMethods
        #endregion Methods
    }
}
