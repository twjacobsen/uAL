using System;
using System.IO;
using System.Configuration;

namespace uAL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Settings settings = new Settings();
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
                settings.Save();
            }
            Console.Clear();
            Console.WriteLine("Connecting to " + settings.Host);

            try
            {
                TorrentAPI t = new TorrentAPI(settings.Host, settings.UserName, settings.Password);
                FileSystemMonitor fs = new FileSystemMonitor(t);
                Console.WriteLine("Connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to uTorrent. Please exit this program, start uTorrent, and try again.");
            }

            Console.WriteLine("To reset your settings, type RESET. To exit the program, press ENTER");
            string reset = Console.ReadLine();
            if (reset == "RESET")
            {
                settings.Password = "";
                settings.UserName = "";
                settings.Host = "";
                settings.Dir = "";
                settings.Save();
            }
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
    }
}
