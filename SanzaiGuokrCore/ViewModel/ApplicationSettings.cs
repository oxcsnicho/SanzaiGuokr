using System;
using System.IO.IsolatedStorage;
using System.Windows;
using GalaSoft.MvvmLight;
using Microsoft.Phone.Info;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.SinaApiV2;
using WeiboApi;

namespace SanzaiGuokr.ViewModel
{
    public class ApplicationSettingsViewModel : ViewModelBase
    {
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

        IsolatedStorageSettings settings;

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

        public void Save()
        {
            settings.Save();
        }

        #region FontSize
        const string FontSizeSettingKeyName = "FontSizeSetting";
        public enum FontSizeSettingValue
        {
            Normal,
            Large,
            ExtraLarge
        }
        const FontSizeSettingValue FontSizeSettingDefault = FontSizeSettingValue.Large;
        const string FontSizeSettingNormal = "/SanzaiGuokr;component/Styles/FontSizeNormal.xaml";
        const string FontSizeSettingLarge = "/SanzaiGuokr;component/Styles/FontSizeLarge.xaml";
        const string FontSizeSettingExtraLarge = "/SanzaiGuokr;component/Styles/FontSizeExtraLarge.xaml";
        const string FontSizeSettingBoolPropertyName = "FontSizeSettingBool";
        public FontSizeSettingValue FontSizeSettingEnum
        {
            get
            {
                if (IsInDesignMode)
                    return FontSizeSettingDefault;
                else
                    return GetValueOrDefault<FontSizeSettingValue>(FontSizeSettingKeyName, FontSizeSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(FontSizeSettingKeyName, value))
                {
                    Save();
                    SettingsChanged(FontSizeSettingBoolPropertyName);
                    SettingsChanged(FontSizeSettingDisplayStringPropertyName);
                }
            }
        }
        public const string FontSizeSettingPropertyName = "FontSizeSetting";
        public string FontSizeSetting
        {
            get
            {
                switch (FontSizeSettingEnum)
                {
                    case FontSizeSettingValue.Normal:
                        return FontSizeSettingNormal;
                    case FontSizeSettingValue.Large:
                        return FontSizeSettingLarge;
                    case FontSizeSettingValue.ExtraLarge:
                        return FontSizeSettingExtraLarge;
                    default:
                        return FontSizeSettingNormal;
                }
            }
        }
        public const string FontSizeSettingDisplayStringPropertyName = "FontSizeSettingDisplayString";
        public string FontSizeSettingDisplayString
        {
            get
            {
                switch (FontSizeSettingEnum)
                {
                    case FontSizeSettingValue.Normal:
                        return "适合视力5.0的你";
                    case FontSizeSettingValue.Large:
                        return "适合眼镜600度的你";
                    case FontSizeSettingValue.ExtraLarge:
                        return "适合喜欢横着看的你\n(不过横屏尚未支持...)";
                    default:
                        return "";
                }
            }
        }
        #endregion

        #region ColorTheme
        const string ColorThemeDay = "/SanzaiGuokr;component/Styles/Day.xaml";
        const string ColorThemeNight = "/SanzaiGuokr;component/Styles/Night.xaml";
        public string ColorThemeUri
        {
            get
            {
                switch (AlwaysEnableDarkTheme)
                {
                    case ColorThemeOptions.Auto:
                        if (DateTime.Now.Hour > 22 || DateTime.Now.Hour < 6)
                            return ColorThemeNight;
                        else
                            return ColorThemeDay;
                    case ColorThemeOptions.AlwaysDay:
                        return ColorThemeDay;
                    case ColorThemeOptions.AlwaysNight:
                        return ColorThemeNight;
                    default:
                        return ColorThemeDay;
                }
            }
        }
        public enum ColorThemeOptions { Auto, AlwaysDay, AlwaysNight };
        public enum ColorThemeMode { Day, Night } ;
        public ColorThemeMode ColorThemeStatus
        {
            get
            {
                if (ColorThemeUri == ColorThemeDay)
                    return ColorThemeMode.Day;
                else
                    return ColorThemeMode.Night;
            }
        }
        const string AlwaysEnableDarkThemePropertyName = "AlwaysEnableDarkTheme";
        const ColorThemeOptions AlwaysEnableDarkThemeDefault = ColorThemeOptions.Auto;
        public ColorThemeOptions AlwaysEnableDarkTheme
        {
            get
            {
                if (IsInDesignMode)
                    return AlwaysEnableDarkThemeDefault;
                else
                    return GetValueOrDefault<ColorThemeOptions>(AlwaysEnableDarkThemePropertyName, AlwaysEnableDarkThemeDefault);
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
                switch (AlwaysEnableDarkTheme)
                {
                    case ColorThemeOptions.Auto:
                        return "夜间自动变色，床阅最爱";
                    case ColorThemeOptions.AlwaysDay:
                        return "只用白天模式，忠于原色";
                    case ColorThemeOptions.AlwaysNight:
                        return "只用夜晚模式，省电养眼";
                    default:
                        return "有bug";
                }
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
                Deployment.Current.Dispatcher.BeginInvoke(() => RaisePropertyChanged(SettingsChangedPropertyName));
            }
        }
        private void SettingsChanged(string name)
        {
            if (name == FontSizeSettingBoolPropertyName
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
                    SettingsChanged(WeiboAccountProfilePropertyName);
                    SettingsChanged(WeiboButtonActionPropertyName);
                }
            }
        }
        const string WeiboAccountAccessTokenPropertyName = "WeiboAccountAccessToken";
        public string WeiboAccountAccessToken
        {
            get
            {
                return WeiboAccountSinaLogin.access_token;
            }
        }
        const string WeiboAccountProfilePropertyName = "WeiboAccountProfile";
        private user _weiboProfile = null;
        public user WeiboAccountProfile
        {
            get
            {
                if (!WeiboAccountLoginStatus)
                    return null;

                if (_weiboProfile == null)
                {
                    _weiboProfile = new user();
                    _weiboProfile.name = WeiboAccountSinaLogin.username;
                }
                return _weiboProfile;
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
        private string anonymousUserId;
        public string AnonymousUserId
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(anonymousUserId))
                    {
                        string anid = UserExtendedProperties.GetValue("ANID") as string;
                        anonymousUserId = anid.Substring(2, 32); // in case anid is null, exception will be thrown which is desired
                    }
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
                return GuokrAccountProfile != null && !string.IsNullOrEmpty(GuokrAccountProfile.access_token);
            }
        }

        const string GuokrRnNumberPropertyName = "GuokrRnNumber";
        private GuokrApiV2.GuokrRnNum _rnnum = new GuokrApiV2.GuokrRnNum() { r = 0, n = 0 };
        public GuokrApiV2.GuokrRnNum GuokrRnNumber
        {
            get
            {
                return _rnnum;
            }
            set
            {
                if (value.TotalValue == _rnnum.TotalValue)
                    return;
                _rnnum.r = value.r;
                _rnnum.n = value.n;
                SettingsChanged(GuokrRnNumberPropertyName);
                SettingsChanged(GuokrRnStringPropertyName);
            }
        }
        const string GuokrRnStringPropertyName = "GuokrRnString";
        public string GuokrRnString
        {
            get
            {
#if false
                if (_rnnum.SumValue <= 1)
                    return "有消息~";
                else
#endif
                return "(" + _rnnum.SumValue.ToString() + ")";
            }
        }
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
                    SettingsChanged(GuokrButtonActionPropertyName);
                    SettingsChanged(IsGroupEnabledSettingDisplayStringPropertyName);
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
                return !GuokrAccountLoginStatus ? "未登录" : (GuokrAccountProfile != null && !string.IsNullOrEmpty(GuokrAccountProfile.nickname) ? GuokrAccountProfile.nickname : "已登录");
            }
        }

        #endregion

        #region group

        const string IsGroupEnabledSettingKeyName = "IsGroupEnabled";
        const bool IsGroupEnabledSettingDefault = true;
        const string IsGroupEnabledSettingBoolPropertyName = "IsGroupEnabledSettingBool";
        public bool IsGroupEnabledSettingBool
        {
            get
            {
                return GetValueOrDefault<bool>(IsGroupEnabledSettingKeyName, IsGroupEnabledSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(IsGroupEnabledSettingKeyName, value))
                {
                    Save();
                    SettingsChanged(IsGroupEnabledSettingDisplayStringPropertyName);
                    SettingsChanged(IsGroupEnabledSettingBoolPropertyName);
                }
            }
        }
        const string IsGroupEnabledSettingDisplayStringPropertyName = "IsGroupEnabledSettingDisplayString";
        public string IsGroupEnabledSettingDisplayString
        {
            get
            {
                if (IsGroupEnabledSettingBool)
                    if (GuokrAccountLoginStatus)
                        return "开启(显示你的小组)";
                    else
                        return "开启(显示热门帖子)";
                else
                    return "关闭";
            }
        }
        #endregion

        #region network status
        public enum NetworkType
        {
            CELLULAR,
            WIFI
        };
        const string NetworkStatusPropertyName = "NetworkStatus";
        private NetworkType ns = NetworkType.CELLULAR;
        public NetworkType NetworkStatus
        {
            get
            {
                if (LoadIcon)
                    return NetworkType.WIFI;
                else
                    return ns;
            }
            set
            {
                if (ns != value)
                {
                    ns = value;
                    SettingsChanged(NetworkStatusPropertyName);
                }
            }
        }
        const string LoadIconPropertyName = "LoadIcon";
        const bool LoadIconDefault = false;
        private bool _li = false;
        public bool LoadIcon
        {
            get
            {
                return GetValueOrDefault<bool>(LoadIconPropertyName, LoadIconDefault);
            }
            set
            {
                if (AddOrUpdateValue(LoadIconPropertyName, value))
                {
                    Save();
                    SettingsChanged(LoadIconPropertyName);
                    SettingsChanged(LoadIconDisplayStringPropertyName);
                }
            }
        }
        const string LoadIconDisplayStringPropertyName = "LoadIconDisplayString";
        public string LoadIconDisplayString
        {
            get
            {
                if (LoadIcon)
                    return "全部读取（使用流量）";
                else
                    return "只在wifi下读取（节省流量）";
            }
        }

        #endregion

        #region random gate
        const string MaxArticleNumberPropertyName = "MaxArticleNumber";
        private int MaxArticleNumberDefault = 7000;
        public int MaxArticleNumber
        {
            get
            {
                return GetValueOrDefault<int>(MaxArticleNumberPropertyName, MaxArticleNumberDefault);
            }
            set
            {
                if (AddOrUpdateValue(MaxArticleNumberPropertyName, value))
                    Save();
            }
        }
        #endregion
    }
}