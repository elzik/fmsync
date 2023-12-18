using System.Text.Json;
using System.IO;
using WixToolset.Dtf.WindowsInstaller;
using Serilog.Configuration;

namespace Elzik.FmSync.WindowsInstaller.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult AddPollyDebugLevelOverride(Session session)
        {
            var appSettingsPath = Path.Combine(
                session.CustomActionData["WorkerInstallFolder"], 
                "appSettings.json");

            var appSettingsContents = File.ReadAllText(appSettingsPath);

            var thing = JsonSerializer.Deserialize<LoggerMinimumLevelConfiguration>(appSettingsContents);

            session.Log($"Custom action is saving test file: {appSettingsPath}");
            File.WriteAllText(appSettingsPath, "Testing...");

            return ActionResult.Success;
        }
    }
}
