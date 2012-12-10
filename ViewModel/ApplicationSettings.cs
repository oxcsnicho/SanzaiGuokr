using System;
using System.IO.IsolatedStorage;
using GalaSoft.MvvmLight;
using System.Windows;
using WeiboApi;
using SanzaiGuokr.SinaApiV2;
using Microsoft.Phone.Info;
using SanzaiGuokr.GuokrObject;
using RestSharp;
using System.Collections.Generic;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using SanzaiGuokr.Util;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;

namespace SanzaiGuokr.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class ApplicationSettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationSettings class.
        /// </summary>
        public ApplicationSettingsViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
                settings = IsolatedStorageSettings.ApplicationSettings;

                if (settings.Contains(DebugModePropertyName))
                    settings[DebugModePropertyName] = false;
            }
        }

        // Our isolated storage settings
        IsolatedStorageSettings settings;

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        public bool AddOrUpdateCookies(string Key, GuokrCookie value)
        {
            bool valueChanged = false;

            if (settings.Contains(Key))
            {
                if (settings[Key] != value)
                {
                    settings[Key] = value;
                    AddOrUpdateValue(Key + "Raw", CookieSerializer.Serialize(value == null ? null : value.CookieContainer));
                    valueChanged = true;
                }
            }
            else
            {
                settings.Add(Key, value);
                AddOrUpdateValue(Key + "Raw", CookieSerializer.Serialize(value.CookieContainer));
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public GuokrCookie GetCookiesOrDefault(string Key, GuokrCookie defaultValue)
        {
            GuokrCookie value = null;

            if (settings.Contains(Key))
                value = (GuokrCookie)settings[Key];

            if ((value == null || value.CookieContainer == null || value.CookieContainer.Count == 0) && settings.Contains(Key + "Raw"))
            {
                value = new GuokrCookie();
                string raw = (string)settings[Key + "Raw"];
                value.CookieContainer = CookieSerializer.Deserialize(raw);
                if (settings.Contains(Key))
                    settings[Key] = value;
                else
                    settings.Add(Key, value);
            }

            if (value == null)
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            settings.Save();
        }

        #region FontSize
        const string FontSizeSettingKeyName = "FontSizeIsLarge";
        const bool FontSizeSettingDefault = false;
        const string FontSizeSettingNormal = "Styles/FontSizeNormal.xaml";
        const string FontSizeSettingLarge = "Styles/FontSizeLarge.xaml";
        public bool FontSizeSettingBool
        {
            get
            {
                return GetValueOrDefault<bool>(FontSizeSettingKeyName, FontSizeSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(FontSizeSettingKeyName, value))
                {
                    Save();
                    SettingsChanged(FontSizeSettingDisplayStringPropertyName);
                }
            }
        }
        public const string FontSizeSettingPropertyName = "FontSizeSetting";
        public string FontSizeSetting
        {
            get
            {
                switch (FontSizeSettingBool)
                {
                    case false:
                        return FontSizeSettingNormal;
                    case true:
                        return FontSizeSettingLarge;
                    default:
                        return "";
                }
            }
        }
        public const string FontSizeSettingDisplayStringPropertyName = "FontSizeSettingDisplayString";
        public string FontSizeSettingDisplayString
        {
            get
            {
                switch (FontSizeSettingBool)
                {
                    case false:
                        return "稍小";
                    case true:
                        return "稍大";
                    default:
                        return "";
                }
            }
        }
        #endregion

        #region ColorTheme
        const string ColorThemeDay = "Styles/Day.xaml";
        const string ColorThemeNight = "Styles/Night.xaml";
        public string ColorThemeUri
        {
            get
            {
                if (AlwaysEnableDarkTheme)
                {
                    return ColorThemeNight;
                }
                else if (DateTime.Now.Hour > 22 || DateTime.Now.Hour < 6)
                {
                    return ColorThemeNight;
                }
                else
                    return ColorThemeDay;
            }
        }
        public enum ColorThemeMode { DAY, NIGHT } ;
        public ColorThemeMode ColorThemeStatus
        {
            get
            {
                if (ColorThemeUri == ColorThemeDay)
                    return ColorThemeMode.DAY;
                else
                    return ColorThemeMode.NIGHT;
            }
        }
        const string AlwaysEnableDarkThemePropertyName = "AlwaysEnableDarkTheme";
        const bool AlwaysEnableDarkThemeDefault = false;
        public bool AlwaysEnableDarkTheme
        {
            get
            {
                return GetValueOrDefault<bool>(AlwaysEnableDarkThemePropertyName, AlwaysEnableDarkThemeDefault);
            }
            set
            {
                if (AddOrUpdateValue(AlwaysEnableDarkThemePropertyName, value))
                {
                    Save();
                    SettingsChanged(AlwaysEnableDarkThemePropertyName);
                    SettingsChanged(AlwaysEnableDarkThemeDisplayStringPropertyName);
                }
            }

        }
        const string AlwaysEnableDarkThemeDisplayStringPropertyName = "AlwaysEnableDarkThemeDisplayString";
        public string AlwaysEnableDarkThemeDisplayString
        {
            get
            {
                return AlwaysEnableDarkTheme ? "强制使用 (省电)" : "自动 (夜间开启)";
            }
        }
        #endregion

        #region debug mode
        const string DebugModePropertyName = "DebugMode";
        const bool DebugModeDefault = false;
        public bool DebugMode
        {
            get
            {
                return GetValueOrDefault<bool>(DebugModePropertyName, DebugModeDefault);
            }
            set
            {
                if (AddOrUpdateValue(DebugModePropertyName, value))
                {
                    // DebugMode does not go to persistence
                    // Save();
                    SettingsChanged(DebugModePropertyName);
                    SettingsChanged(DebugModeDisplayStringPropertyName);
                }
            }

        }
        const string DebugModeDisplayStringPropertyName = "DebugModeDisplayString";
        public string DebugModeDisplayString
        {
            get
            {
                return DebugMode ? "开启（正在收集debug信息）" : "关闭";
            }
        }

        #endregion

        #region SettingsChanged
        public const string SettingsChangedPropertyName = "IsSettingsChanged";
        private bool _sc = false;
        public bool IsSettingsChanged
        {
            get
            {
                return _sc;
            }
            set
            {
                if (_sc == value)
                    return;
                _sc = value;
                Deployment.Current.Dispatcher.BeginInvoke
                    (() =>
                RaisePropertyChanged(SettingsChangedPropertyName)
            );
            }
        }
        private void SettingsChanged(string name)
        {
            if (name == FontSizeSettingPropertyName
                || name == AlwaysEnableDarkThemePropertyName)
                IsSettingsChanged = true;
            Deployment.Current.Dispatcher.BeginInvoke(() => RaisePropertyChanged(name));
        }
        #endregion

        #region WeiboAccount
        const string WeiboAccountLoginStatusPropertyName = "WeiboAccountLoginStatus";
        const bool WeiboAccountLoginStatusDefault = false;
        public bool WeiboAccountLoginStatus
        {
            get
            {
                return WeiboAccountSinaLogin != null && WeiboAccountSinaLogin.IsValid;
            }
        }
        const string WeiboAccountSinaLoginPropertyName = "WeiboAccountSinaLogin";
        public SinaLogin WeiboAccountSinaLogin
        {
            get
            {
                return GetValueOrDefault<SinaLogin>(WeiboAccountSinaLoginPropertyName, new SinaLogin());
            }
            set
            {
                if (AddOrUpdateValue(WeiboAccountSinaLoginPropertyName, value))
                {
                    Save();
                    SettingsChanged(WeiboAccountSinaLoginPropertyName);
                    SettingsChanged(WeiboAccountLoginStatusPropertyName);
                    SettingsChanged(WeiboAccountAccessTokenPropertyName);
                    SettingsChanged(WeiboButtonActionPropertyName);
                }
            }
        }
        const string WeiboAccountAuthTokenPropertyName = "WeiboAccountAuthToken";
        const string WeiboAccountAuthTokenDefault = "";
        public string WeiboAccountAuthToken
        {
            get
            {
                return GetValueOrDefault<string>(WeiboAccountAuthTokenPropertyName, WeiboAccountAuthTokenDefault);
            }
        }
        const string WeiboAccountAccessTokenPropertyName = "WeiboAccountAccessToken";
        const string WeiboAccountAccessTokenDefault = "";
        public string WeiboAccountAccessToken
        {
            get
            {
                return WeiboAccountSinaLogin.access_token;
            }
        }
        public bool SetupWeiboAccount(bool status, string auth_token, string access_token)
        {
            bool orig_status = WeiboAccountLoginStatus;
            string orig_auth_token = WeiboAccountAuthToken;

            if (AddOrUpdateValue(WeiboAccountLoginStatusPropertyName, status))
            {
                if (AddOrUpdateValue(WeiboAccountAccessTokenPropertyName, access_token))
                {
                    Save();
                    SettingsChanged(WeiboAccountAuthTokenPropertyName);
                    SettingsChanged(WeiboAccountAccessTokenPropertyName);
                    SettingsChanged(WeiboAccountLoginStatusPropertyName);
                    SettingsChanged(WeiboButtonActionPropertyName);
                    SettingsChanged(WeiboAccountNamePropertyName);
                    return true;
                }
            }

            //rollback
            AddOrUpdateValue(WeiboAccountLoginStatusPropertyName, orig_status);
            AddOrUpdateValue(WeiboAccountAuthTokenPropertyName, orig_auth_token);
            return false;
        }
        const string WeiboAccountProfilePropertyName = "WeiboAccountProfile";
        public user WeiboAccountProfile
        {
            get
            {
                return GetValueOrDefault<user>(WeiboAccountProfilePropertyName, new user());
            }
            set
            {
                if (AddOrUpdateValue(WeiboAccountProfilePropertyName, value))
                {
                    Save();
                    SettingsChanged(WeiboAccountProfilePropertyName);
                    SettingsChanged(WeiboAccountNamePropertyName);
                }
            }
        }
        const string WeiboButtonActionPropertyName = "WeiboButtonAction";
        public string WeiboButtonAction
        {
            get
            {
                return WeiboAccountLoginStatus ? "清除" : "登录";
            }
        }
        const string WeiboAccountNamePropertyName = "WeiboAccountName";
        public string WeiboAccountName
        {
            get
            {
                return !WeiboAccountLoginStatus ? "未登录" : (WeiboAccountProfile != null ? WeiboAccountProfile.name : "无用户名");
            }
        }
        #endregion

        #region MrGuokrAccount
        const string MrGuokrSinaLoginPropertyName = "MrGuokrSinaLogin";
        public SinaLogin MrGuokrSinaLogin
        {
            get
            {
                return GetValueOrDefault<SinaLogin>(MrGuokrSinaLoginPropertyName, new SinaLogin());
            }
            set
            {
                if (AddOrUpdateValue(MrGuokrSinaLoginPropertyName, value))
                {
                    Save();
                    SettingsChanged(MrGuokrSinaLoginPropertyName);
                }
            }
        }
        #endregion

        #region AdminLiveId
        public string AnonymousUserId
        {
            get
            {

                try
                {
                    string anid = UserExtendedProperties.GetValue("ANID") as string;
                    string anonymousUserId = anid.Substring(2, 32); // in case anid is null, exception will be thrown which is desired
                    return anonymousUserId;
                }
                catch
                {
                    return "";
                }
            }
        }
        const string IsAdminLiveIdPropertyName = "IsAdminLiveId";
        public bool IsAdminLiveId
        {
            get
            {
                return AnonymousUserId == "1D8D874F9703EB1C4FC0E2F5FFFFFFFF"
                    || AnonymousUserId == "35E1A346BCD794F5F4EC941DFFFFFFFF"
                    || AnonymousUserId == "";
            }
        }
        public string MrGuokrTokenExpireTime
        {
            get
            {
                TimeSpan s = DateTime.FromFileTimeUtc(MrGuokrSinaLogin.request_time_utc).AddSeconds(MrGuokrSinaLogin.expires_in) - DateTime.Now;
                return string.Format("{0}天{1}小时{2}分 后过期", s.Days, s.Hours, s.Minutes);
            }
        }
        #endregion

        #region guokrAccount

        const string GuokrAccountLoginStatusPropertyName = "GuokrAccountLoginStatus";
        const bool GuokrAccountLoginStatusDefault = false;
        public bool GuokrAccountLoginStatus
        {
            get
            {
                return GuokrAccountProfile != null && !string.IsNullOrEmpty(GuokrAccountProfile.username);
            }
        }
#if false
        const string GuokrAccountCookiePropertyName = "GuokrAccountCookie";
        const string GuokrAccountCookieRawPropertyName = "GuokrAccountCookieRaw";
        GuokrCookie GuokrAccountCookieDefault = new GuokrCookie();
        public GuokrCookie GuokrAccountCookie
        {
            get
            {
                return GetCookiesOrDefault(GuokrAccountCookiePropertyName, GuokrAccountCookieDefault);
            }
            set
            {
                if (AddOrUpdateCookies(GuokrAccountCookiePropertyName, value))
                {
                    Save();
                    SettingsChanged(GuokrAccountCookiePropertyName);
                    SettingsChanged(GuokrAccountLoginStatusPropertyName);
                    SettingsChanged(GuokrButtonActionPropertyName);
                }
            }
        }
#endif

        const string GuokrAccountProfilePropertyName = "GuokrAccountProfile";
        public GuokrUserLogin GuokrAccountProfile
        {
            get
            {
                return GetValueOrDefault<GuokrUserLogin>(GuokrAccountProfilePropertyName, new GuokrUserLogin());
            }
            set
            {
                if (AddOrUpdateValue(GuokrAccountProfilePropertyName, value))
                {
                    Save();
                    SettingsChanged(GuokrAccountProfilePropertyName);
                    SettingsChanged(GuokrAccountLoginStatusPropertyName);
                    SettingsChanged(GuokrAccountNamePropertyName);
                }
            }
        }
        const string GuokrButtonActionPropertyName = "GuokrButtonAction";
        public string GuokrButtonAction
        {
            get
            {
                return GuokrAccountLoginStatus ? "清除" : "登录";
            }
        }
        const string GuokrAccountNamePropertyName = "GuokrAccountName";
        public string GuokrAccountName
        {
            get
            {
                return !GuokrAccountLoginStatus ? "未登录" : (GuokrAccountProfile != null ? GuokrAccountProfile.nickname : "无用户名");
            }
        }

        #endregion
    }
}