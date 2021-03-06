﻿using HES.Core.Interfaces;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class LogsViewerService : ILogsViewerService
    {
        private readonly IJSRuntime _jSRuntime;
        private readonly string _folderPath;

        public LogsViewerService(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
            _folderPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "logs");
        }

        public List<string> GetFiles()
        {
            return new DirectoryInfo(_folderPath).GetFiles("*.log").Select(s => s.Name).OrderByDescending(x => x).ToList();
        }

        public async Task DownloadLogAsync(string name)
        {
            var path = Path.Combine(_folderPath, name);
            var content = File.ReadAllText(path);
            await _jSRuntime.InvokeVoidAsync("downloadLog", name, content);
        }

        public async Task<List<LogModel>> GetLogAsync(string name)
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
                    list.Add(new LogModel { Name = name, Date = separator + " " + item.Split("|")[0], Level = item.Split("|")[1], Logger = item.Split("|")[2], Message = item.Split("|")[3], Method = item.Split("|")[4], Url = item.Split("|")[5] });
                }
            }

            return list.OrderByDescending(x => x.Date).ToList();
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
        public string Name { get; set; }
        public string Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
    }
}