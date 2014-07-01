#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Windows.Storage;
using Newtonsoft.Json;

#endregion

namespace DogeMuch.Utility
{
    /// <summary>
    ///     Collection of string constants used in the entire solution. This file is shared for all projects
    /// </summary>
    internal static class ApplicationSettingsHelper
    {
        /// <summary>
        ///     Function to read a setting value
        /// </summary>
        public static object ReadSettingsValue(string key)
        {
            return _ReadResetSettingsValue<object>(key, false);
        }


        /// <summary>
        ///     Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadResetSettingsValue(string key)
        {
            return _ReadResetSettingsValue<object>(key, reset: true);
        }

        private static T _ReadResetSettingsValue<T>(string key, bool reset)
        {
            Debug.WriteLine(key);
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                Debug.WriteLine("null returned");
                return default(T);
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                if (reset)
                    ApplicationData.Current.LocalSettings.Values.Remove(key);
                Debug.WriteLine("value found " + value.ToString());


                return (T) value;
            }
        }


        /// <summary>
        ///     Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value)
        {
            Debug.WriteLine(key + ":" + value.ToString());
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
    }
}
