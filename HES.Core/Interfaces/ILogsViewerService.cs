using HES.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILogsViewerService
    {
        List<string> GetLogFiles();
        Task<List<LogModel>> GetSelectedLog(string name);
        string GetFilePath(string name);
        void DeleteFile(string name);
    }
}