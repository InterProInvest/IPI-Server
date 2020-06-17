using HES.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILogsViewerService
    {
        List<string> GetFiles();
        Task<List<LogModel>> GetLogAsync(string name);
        Task DownloadLogAsync(string name);
        string GetFilePath(string name);
        void DeleteFile(string name);
    }
}