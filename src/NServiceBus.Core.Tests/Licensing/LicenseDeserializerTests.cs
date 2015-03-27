namespace NServiceBus.Core.Tests.Licensing
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using NServiceBus.Licensing;
    using NUnit.Framework;

    [TestFixture]
    public class LicenseDeserializerTests
    {

        [Test]
        [Explicit]
        public void ParseAllTheLicenses()
        {
            //Set an environment variable to a path containing all the licenses to run this test
            var allTheLicensesDir = Environment.GetEnvironmentVariable("NServiceBusLicensesPath");
            foreach (var licensePath in Directory.EnumerateFiles(allTheLicensesDir, "license.xml", SearchOption.AllDirectories))
            {
                var licenseManager = LicenseDeserializer.Deserialize(File.ReadAllText(licensePath));
                Debug.WriteLine(licenseManager.UpgradeProtectionExpiration);
            }
        }

        [Test]
        public void WithAllProperties()
        {
            var license = LicenseDeserializer.Deserialize(ResourceReader.ReadResourceAsString("Licensing.LicenseWithAllProperties.xml"));

            var dateTimeOffset = new DateTime(2014, 2, 6, 0, 0, 0,DateTimeKind.Utc);
            Assert.AreEqual(dateTimeOffset, license.ExpirationDate);
            Assert.AreEqual(int.MaxValue, license.AllowedNumberOfWorkerNodes);
            Assert.AreEqual(0, license.MaxThroughputPerSecond);
            Assert.AreEqual(new DateTime(2013, 11, 6, 0, 0, 0,DateTimeKind.Utc), license.UpgradeProtectionExpiration);
        }

        [Test]
        public void WithNoUpgradeProtection()
        {
            var license = LicenseDeserializer.Deserialize(ResourceReader.ReadResourceAsString("Licensing.LicenseWithNoUpgradeProtection.xml"));
            Assert.IsNull(license.UpgradeProtectionExpiration);
        }

        [Test]
        public void WithNonMaxThresholds()
        {
            var license = LicenseDeserializer.Deserialize(ResourceReader.ReadResourceAsString("Licensing.LicenseWithNonMaxThresholds.xml"));
            Assert.AreEqual(2, license.AllowedNumberOfWorkerNodes);
            Assert.AreEqual(2, license.MaxThroughputPerSecond);
        }

        [Test]
        public void Should_parse_licenses_when_calendar_settings_are_different_from_nservicebus_default()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");

            var licenses = new[]
            {
                ResourceReader.ReadResourceAsString("Licensing.LicenseWithAllProperties.xml"),
                ResourceReader.ReadResourceAsString("Licensing.LicenseWithNoUpgradeProtection.xml"),
                ResourceReader.ReadResourceAsString("Licensing.LicenseWithNonMaxThresholds.xml")
            };

            foreach (var license in licenses)
            {
                LicenseDeserializer.Deserialize(license);
            }
        }
    }
}