using System;

namespace Elzik.FmSync.Worker.ConfigurationUpgrade
{
    public class ConfigurationUpgrades
    {
        public static void AddPollyDebugLevelOverride(string settingsFilePath)
        {
            throw new  NotImplementedException("When implemented, this will add a Serilog loggign level override for Polly to avoid " +
                "logging every warning when a retry is made.");
        }
    }
}
