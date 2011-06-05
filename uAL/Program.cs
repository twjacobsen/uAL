using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;

namespace uAL
{
    class Program
    {
        public static Settings settings = new Settings();
        public static List<string> Labels;
        public static TorrentAPI t;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            if (settings.UserName == "" && settings.Password == "")
            {
                Console.WriteLine("This is your first time starting this application.");
                Console.WriteLine("Please provide some basic uTorrent settings.");
                Console.Write("Username: ");
                settings.UserName = Console.ReadLine();
                Console.Write("Password: ");
                settings.Password = Console.ReadLine();
                Console.Write("Hostname (fx. localhost:8080): ");
                settings.Host = Console.ReadLine();
                settings.Host = settings.Host.Replace("localhost", "127.0.0.1");
                if (!settings.Host.Contains("127.0.0.1"))
                {
                    Console.WriteLine("You have chosen to run the labeller from an computer other than the one uTorrent is running on.");
                    Console.WriteLine("You have to specify which directory to look for torrents in.");
                    Console.Write("Dir: ");
                    settings.Dir = Console.ReadLine();
                    while (!Directory.Exists(settings.Dir))
                    {
                        Console.WriteLine("Invalid dir, try again.");
                        Console.Write("Dir: ");
                        settings.Dir = Console.ReadLine();
                    }
                }
                else
                {
                    settings.Dir = "";
                }
                Console.Write("Should automatically added torrents be stopped when done? (Y/N) ");
                settings.StopOnDone = Console.ReadLine();
                while (settings.StopOnDone.ToLower() != "y" && settings.StopOnDone.ToLower() != "n")
                {
                    Console.Write("Invalid input, only Y or N allowed. Try again: ");
                    settings.StopOnDone = Console.ReadLine();
                }
                settings.Save();
            }
            Console.Clear();
            Console.WriteLine("Connecting to " + settings.Host);

            try
            {
                t = new TorrentAPI(settings.Host, settings.UserName, settings.Password);
                if (settings.Dir == "")
                    settings.Dir = t.GetDownloadDir();

                Console.WriteLine("Detecting label folders...");
                Console.WriteLine("Detected: ");
                Labels = new List<string>();
                DirectoryInfo d = new DirectoryInfo(settings.Dir);
                foreach (DirectoryInfo label in d.GetDirectories())
                {
                    Console.WriteLine(label.Name);
                    Labels.Add(label.Name);
                }
                FileSystemMonitor fs = new FileSystemMonitor();
                Console.WriteLine("Connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to uTorrent. Please exit this program, start uTorrent, and try again.");
                Console.WriteLine("########");
                Console.WriteLine("# Error: " + ex.Message);
                Console.WriteLine("# Source: " + ex.Source);
                Console.WriteLine("########");
                Console.WriteLine();
            }

            Console.WriteLine("To reset your settings, type RESET. To exit the program, press ENTER");
            string reset = Console.ReadLine();
            if (reset == "RESET")
            {
                settings.Password = "";
                settings.UserName = "";
                settings.Host = "";
                settings.Dir = "";
                settings.StopOnDone = "";
                settings.Save();
                return;
            }
            settings.Save();
        }
    }

    sealed class Settings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string UserName
        {
            get { return (string)(this["UserName"]); }
            set { this["UserName"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string Password
        {
            get { return (string)(this["Password"]); }
            set { this["Password"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string Host
        {
            get { return (string)(this["Host"]); }
            set { this["Host"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string Dir
        {
            get { return (string)(this["Dir"]); }
            set { this["Dir"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string StopOnDone
        {
            get { return (string)(this["StopOnDone"]); }
            set { this["StopOnDone"] = value; }
        }
    }
}
