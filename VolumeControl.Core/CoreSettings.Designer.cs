﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VolumeControl.Core {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.3.0")]
    public sealed partial class CoreSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static CoreSettings defaultInstance = ((CoreSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new CoreSettings())));
        
        public static CoreSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SelectedSession {
            get {
                return ((string)(this["SelectedSession"]));
            }
            set {
                this["SelectedSession"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3000")]
        public int ReloadInterval_ms {
            get {
                return ((int)(this["ReloadInterval_ms"]));
            }
            set {
                this["ReloadInterval_ms"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3000")]
        public int ReloadInterval_ms_Default {
            get {
                return ((int)(this["ReloadInterval_ms_Default"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000")]
        public int ReloadInterval_ms_Min {
            get {
                return ((int)(this["ReloadInterval_ms_Min"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("120000")]
        public int ReloadInterval_ms_Max {
            get {
                return ((int)(this["ReloadInterval_ms_Max"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LockSelectedSession {
            get {
                return ((bool)(this["LockSelectedSession"]));
            }
            set {
                this["LockSelectedSession"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection Hotkeys {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Hotkeys"]));
            }
            set {
                this["Hotkeys"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <string>Volume Up::VolumeUp::Volume Up</string>
  <string>Volume Down::VolumeDown::Volume Down</string>
  <string>Toggle Mute::VolumeMute::Toggle Mute</string>
  <string>Next Session::Ctrl+Shift+Alt+E::Next Session</string>
  <string>Previous Session::Ctrl+Shift+Alt+Q::Previous Session</string>
  <string>Lock Session::Ctrl+Shift+Alt+S::Toggle Session Lock</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection Hotkeys_Default {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Hotkeys_Default"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int VolumeStepSize {
            get {
                return ((int)(this["VolumeStepSize"]));
            }
            set {
                this["VolumeStepSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ReloadOnHotkey {
            get {
                return ((bool)(this["ReloadOnHotkey"]));
            }
            set {
                this["ReloadOnHotkey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ReloadOnInterval {
            get {
                return ((bool)(this["ReloadOnInterval"]));
            }
            set {
                this["ReloadOnInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CheckAllDevices {
            get {
                return ((bool)(this["CheckAllDevices"]));
            }
            set {
                this["CheckAllDevices"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LockSelectedDevice {
            get {
                return ((bool)(this["LockSelectedDevice"]));
            }
            set {
                this["LockSelectedDevice"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int VolumeStepSize_Default {
            get {
                return ((int)(this["VolumeStepSize_Default"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SelectedDevice {
            get {
                return ((string)(this["SelectedDevice"]));
            }
            set {
                this["SelectedDevice"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3000")]
        public int ReloadOnHotkey_MinInterval {
            get {
                return ((int)(this["ReloadOnHotkey_MinInterval"]));
            }
            set {
                this["ReloadOnHotkey_MinInterval"] = value;
            }
        }
    }
}
