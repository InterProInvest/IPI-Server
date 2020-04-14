using System.Reflection;

namespace HES.Core.Constants
{
    public class ServerConstants
    {
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        public const string DefaulAccessProfileId = "default";
    }
}