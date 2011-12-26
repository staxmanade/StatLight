using System;
using System.Collections.Generic;
using System.Configuration;
using StatLight.Core.Common;
using StatLight.Core.Properties;

namespace StatLight.Core.Configuration
{
    public class SettingsOverrideApplicator
    {
        private readonly ILogger _logger;

        public SettingsOverrideApplicator(ILogger logger)
        {
            _logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "settingName")]
        public void ApplySettingsFrom(IDictionary<string, string> overriddenSettings, SettingsBase settings)
        {
            if (overriddenSettings == null) throw new ArgumentNullException("overriddenSettings");
            if (settings == null) throw new ArgumentNullException("settings");

            foreach (var overriddenSetting in overriddenSettings)
            {
                var settingName = overriddenSetting.Key;
                var settingStringValue = overriddenSetting.Value;

                var propertyInfo = typeof (Settings).GetProperty(settingName);
                if (propertyInfo == null)
                {
                    // TODO: exception should reflect and print all setting options and their System.Type.
                    throw new StatLightException("Unknown setting value of settingName");
                }

                var type = propertyInfo.PropertyType;

                var settingValue = ParseValue(settingStringValue, type);

                _logger.Debug("Overriding setting default value for [{0}] of [{1}] to [{2}]."
                    .FormatWith(settingName, settings[settingName], settingValue));

                settings[settingName] = settingValue;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.Parse(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.Parse(System.String)")]
        private static object ParseValue(string settingStringValue, Type type)
        {
            object settingValue;
            if (type == typeof (TimeSpan))
            {
                settingValue = TimeSpan.Parse(settingStringValue);
            }
            else if (type == typeof (ConsoleColor))
            {
                settingValue = Enum.Parse(typeof (ConsoleColor), settingStringValue);
            }
            else if (type == typeof (string))
            {
                settingValue = settingStringValue;
            }
            else if (type == typeof (int))
            {
                settingValue = int.Parse(settingStringValue);
            }
            else
            {
                throw new StatLightException("Setting with type of [{0}] not yet supported".FormatWith(type.Name));
            }
            return settingValue;
        }
    }
}