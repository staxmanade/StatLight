using System;
using System.Collections.Generic;
using System.Globalization;

namespace StatLight.Client.Model.Messaging
{
    public interface IQueryStringReader
    {
        T GetValue<T>(string key) where T : IConvertible;
        T GetValueOrDefault<T>(string key, T defaultValue) where T : IConvertible;
    }

    public class QueryStringReader : IQueryStringReader
    {
        /// <summary>
        /// Returns a strongly-typed value from the browsers querystring
        /// </summary>
        /// <typeparam name="T">Type of the value to return</typeparam>
        /// <param name="key">Key in config file for value</param>
        /// <returns>Value from config file (as type T)</returns>
        public T GetValue<T>(string key) where T : IConvertible
        {
            if (key == null)
                throw new ArgumentException("key cannot be null!");

            string actualValue;

            if (TryReadQuerystringValue(key, out actualValue))
            {
                return ConvertToTypedValue<T>(key, actualValue);
            }

            throw new KeyNotFoundException("Could not find a querystring parameter with key [{0}]".FormatWith(key));
        }

        /// <summary>
        /// Returns a strongly-typed value from the browsers querystring, or 
        /// returns the defaultValue supplied if the key is not in the querystring
        /// </summary>
        /// <typeparam name="T">Type of the value to return</typeparam>
        /// <param name="key">Key in config file for value</param>
        /// <param name="defaultValue">Value to return if key is not found in config file</param>
        /// <returns>Value from config file, or defaultValue if key not in config file</returns>
        public T GetValueOrDefault<T>(string key, T defaultValue) where T : IConvertible
        {
            if (key == null)
                throw new ArgumentException("key cannot be null!");

            string actualValue;

            if (TryReadQuerystringValue(key, out actualValue))
            {
                return ConvertToTypedValue<T>(key, actualValue);
            }

            return defaultValue;
        }

        private static T ConvertToTypedValue<T>(string key, string objectValue)
        {
            T value;
            try
            {
                value = (T)Convert.ChangeType(objectValue, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException("Value from querystring [" + objectValue + "] with key [" + key + "] is not convertible to type T [" + typeof(T).FullName + "].", ex);
            }
            return value;
        }


        private static bool TryReadQuerystringValue(string key, out string value)
        {
#if WINDOWS_PHONE
            throw new NotImplementedException();
#else
            var qs = System.Windows.Browser.HtmlPage.Document.QueryString;
            if (qs.ContainsKey(key))
            {
                value = qs[key];
                return true;
            }
            value = null;
            return false;
#endif
        }
    }

}