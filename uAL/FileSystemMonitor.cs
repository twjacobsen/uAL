using System;
using System.IO;

namespace uAL
{
    class FileSystemMonitor
    {
        public FileSystemMonitor(TorrentAPI t)
        {
            string downloadDir = t.GetDownloadDir();
            if (!Directory.Exists(downloadDir))
                downloadDir = @"C:\Users\Administrator\Desktop";
            string downloadFolder = downloadDir.Substring(downloadDir.LastIndexOf('\\') + 1);
            FileSystemWatcher w = new FileSystemWatcher(downloadDir);
            w.Filter = "*.torrent";
            w.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            w.EnableRaisingEvents = true;
            w.IncludeSubdirectories = true;

            w.Created += (s, e) =>
            {
                FileInfo fi = new FileInfo(e.FullPath);
                string tmp = fi.Directory.ToString();
                string eventParent = tmp.Substring(tmp.LastIndexOf('\\') + 1);
                if (downloadFolder != eventParent)
                    if(t.AddTorrent(e.FullPath, eventParent))
                        Path.ChangeExtension(e.FullPath, "loaded");
            };
        }
    }
}
