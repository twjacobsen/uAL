﻿using System;
using System.IO;

namespace uAL
{
    class FileSystemMonitor
    {
        public FileSystemMonitor()
        {
            string downloadDir = uAL.Program.settings.Dir;
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
                    uAL.Program.t.AddTorrent(e.FullPath, eventParent);
            };
        }
    }
}
