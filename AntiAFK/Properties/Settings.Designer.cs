﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AntiAFK.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoExitReturnCheckbox {
            get {
                return ((bool)(this["AutoExitReturnCheckbox"]));
            }
            set {
                this["AutoExitReturnCheckbox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int LogoutIntervalComboBox {
            get {
                return ((int)(this["LogoutIntervalComboBox"]));
            }
            set {
                this["LogoutIntervalComboBox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool RandomCharacterCheckbox {
            get {
                return ((bool)(this["RandomCharacterCheckbox"]));
            }
            set {
                this["RandomCharacterCheckbox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoDesktopNotificationCheckbox {
            get {
                return ((bool)(this["AutoDesktopNotificationCheckbox"]));
            }
            set {
                this["AutoDesktopNotificationCheckbox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoReloginCheckbox {
            get {
                return ((bool)(this["AutoReloginCheckbox"]));
            }
            set {
                this["AutoReloginCheckbox"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomTalkCheckbox_Normal {
            get {
                return ((bool)(this["RandomTalkCheckbox_Normal"]));
            }
            set {
                this["RandomTalkCheckbox_Normal"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomTalkCheckbox_Shout {
            get {
                return ((bool)(this["RandomTalkCheckbox_Shout"]));
            }
            set {
                this["RandomTalkCheckbox_Shout"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomTalkCheckbox_Team {
            get {
                return ((bool)(this["RandomTalkCheckbox_Team"]));
            }
            set {
                this["RandomTalkCheckbox_Team"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomTalkCheckbox_Group {
            get {
                return ((bool)(this["RandomTalkCheckbox_Group"]));
            }
            set {
                this["RandomTalkCheckbox_Group"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RandomTalkContentCheckbox_Robot {
            get {
                return ((bool)(this["RandomTalkContentCheckbox_Robot"]));
            }
            set {
                this["RandomTalkContentCheckbox_Robot"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("To ask why we fight, is to ask why the leaves fall. 我们生而为战，犹如树叶自会飘落。")]
        public string TalkConentTextBox {
            get {
                return ((string)(this["TalkConentTextBox"]));
            }
            set {
                this["TalkConentTextBox"] = value;
            }
        }
    }
}
