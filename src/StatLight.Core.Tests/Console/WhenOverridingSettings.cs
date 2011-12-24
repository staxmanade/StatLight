using System;
using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Properties;

namespace StatLight.Core.Tests.Console.ArgOptionsTests
{
    public class WhenOverridingSettings : FixtureBase
    {
        [Test]
        public void Valid_setting_specified_should_override_value()
        {
            var settings = new Settings();

            var overriddenSettings = new Dictionary<string, string>
                                         {
                                             {"MaxWaitTimeAllowedBeforeCommunicationErrorSent", "00:00:20"},
                                         };

            var settingsOverrideApplicator = new SettingsOverrideApplicator(TestLogger);

            settingsOverrideApplicator.ApplySettingsFrom(overriddenSettings, settings);

            settings.MaxWaitTimeAllowedBeforeCommunicationErrorSent
                .ShouldNotEqual(Settings.Default.MaxWaitTimeAllowedBeforeCommunicationErrorSent);

            settings.MaxWaitTimeAllowedBeforeCommunicationErrorSent.ShouldEqual(TimeSpan.FromSeconds(20));
        }
    }
}