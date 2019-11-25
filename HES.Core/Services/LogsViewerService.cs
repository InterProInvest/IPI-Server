using HES.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class LogsViewerService : ILogsViewerService
    {
        private readonly string _folderPath;

        public LogsViewerService()
        {
            _folderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "logs");
        }

        public List<string> GetLogFiles()
        {
            var list = new List<string>();

            var directoryInfo = new DirectoryInfo(_folderPath);
            FileInfo[] fileInfo = directoryInfo.GetFiles("*.log");

            foreach (var item in fileInfo)
            {
                list.Add(item.Name);
            }

            return list;
        }

        public async Task<List<LogModel>> GetSelectedLog(string name)
        {
            var list = new List<LogModel>();

            var path = Path.Combine(_folderPath, name);
            var text = await File.ReadAllTextAsync(path);
            var separator = name.Substring(8, 10);
            var separated = text.Split(separator);

            foreach (var item in separated)
            {
                if (item != "")
                {
                    list.Add(new LogModel { Date = separator + " " + item.Split("|")[0], Level = item.Split("|")[1], Logger = item.Split("|")[2], Message = item.Split("|")[3], Method = item.Split("|")[4], Url = item.Split("|")[5] });
                }
            }

            return list;
        }

        public string GetFilePath(string name)
        {
            return Path.Combine(_folderPath, name);
        }

        public void DeleteFile(string name)
        {
            var path = Path.Combine(_folderPath, name);
            File.Delete(path);
        }
    }

    public class LogModel
    {
        public string Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
    }
}
