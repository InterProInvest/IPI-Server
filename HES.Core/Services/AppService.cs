using HES.Core.Interfaces;
using System.Reflection;

namespace HES.Core.Services
{
    public class AppService : IAppService
    {
        public string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}