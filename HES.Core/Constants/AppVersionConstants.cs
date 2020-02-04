using System.Reflection;

namespace HES.Core.Constants
{
    public class AppVersionConstants
    {
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}