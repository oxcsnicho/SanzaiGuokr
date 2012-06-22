using System;
using System.IO.IsolatedStorage;
using GalaSoft.MvvmLight;

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
                RaisePropertyChanged(SettingsChangedPropertyName);
            }
        }
        private void SettingsChanged(string name)
        {
            IsSettingsChanged = true;
            RaisePropertyChanged(name);
        }
        #endregion
    }
}