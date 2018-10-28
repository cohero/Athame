using System;
using System.IO;
using System.Windows.Forms;
using Athame.Core.Logging;
using Athame.Core.Plugin;
using Athame.Core.Settings;
using Athame.PluginAPI;
using Athame.Settings;
using Athame.UI;
using Shell32;

namespace Athame
{
    public static class Program
    {







        private const string Tag = nameof(Program);
        public static string LogDir;

        private const string SettingsFilename = "Athame Settings.json";
        private static string SettingsPath;

        public static AthameApplication DefaultApp;
        public static PluginManager DefaultPluginManager;
        public static SettingsFile<AthameSettings> DefaultSettings;
        public static AuthenticationManager DefaultAuthenticationManager = new AuthenticationManager();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {

            //var tempDir = System.IO.Directory.CreateDirectory("Temp");
            //Shell shell = new Shell();
            //Folder folder = shell.NameSpace(tempDir.FullName);
            //var item = folder.ParseName("test.txt");

            //foreach (FolderItem file in folder.Items())
            //{
            //    if (file.Name == "test.txt")
            //    {

            //    }
            //}
            //Folder folder2 = shell.BrowseForFolder(0, "Select a destination for this media:", 0, 0);
            //if (folder == null)
            //{
            //    return;
            //}
            //folder2.CopyHere(item);









            TaskDialogHelper.MainCaption = "Athame";
            // Create app instance config
            DefaultApp = new AthameApplication
            {
                IsWindowed = true
            };

            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Athame");
            var userDataDirArgIndex = Array.IndexOf(args, "--user-data-dir");

            if (userDataDirArgIndex != -1 
                && args.Length <= userDataDirArgIndex + 2)
            {
                var dir = args[userDataDirArgIndex + 1];
                if (Directory.Exists(dir))
                {
                    dataDir = Path.Combine(dir, "Athame Data");
                }
            }
            DefaultApp.UserDataPath = dataDir;


            // Install logging
            LogDir = DefaultApp.UserDataPathOf("Logs");
            Directory.CreateDirectory(LogDir);
            Log.AddLogger("file", new FileLogger(LogDir));
#if !DEBUG
            Log.Filter = Level.Warning;
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Log.WriteException(Level.Fatal, "AppDomain", eventArgs.ExceptionObject as Exception);
            };
#else
            Log.AddLogger("debug", new DebugLogger());
#endif
            Log.Debug(Tag, "Logging installed on AppDomain");

            // Ensure user data dir
            Directory.CreateDirectory(DefaultApp.UserDataPath);

            // Load settings
            SettingsPath = DefaultApp.UserDataPathOf(SettingsFilename);
            DefaultSettings = new SettingsFile<AthameSettings>(SettingsPath);
            DefaultSettings.Load();

            // Create plugin manager instance
            DefaultPluginManager = new PluginManager(Path.Combine(Directory.GetCurrentDirectory(), PluginManager.PluginDir), DefaultApp.UserDataPath);
            
            Log.Debug(Tag, "Ready to begin main form loop");
            // Begin main form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
