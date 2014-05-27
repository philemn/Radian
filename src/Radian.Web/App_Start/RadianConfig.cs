using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Radian
{
    public class RadianConfig
    {
        private static object SyncLock = new object();
        private static FileSystemWatcher Watcher = null;
        private static CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static int _restartDelay = 100;

        public static int RestartDelay
        {
            get { return _restartDelay; }
            set { _restartDelay = value; }
        }

        public static void ConfigureAutoRestart()
        {
            if (Watcher != null) return;

            lock (SyncLock)
            {
                if (Watcher != null) return;

                Watcher = new FileSystemWatcher();
                Watcher.Path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Site");
                Watcher.IncludeSubdirectories = true;
                Watcher.NotifyFilter = 
                        NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;
                Watcher.EnableRaisingEvents = true;
                Watcher.Changed += OnFileSystemChange;
                Watcher.Created += OnFileSystemChange;
                Watcher.Deleted += OnFileSystemChange;
                Watcher.Renamed += OnFileSystemChange;
            }
        }
        
        public static void Unload()
        {
            if (Watcher == null) return;

            lock (SyncLock)
            {
                Watcher.Changed -= OnFileSystemChange;
                Watcher.Created -= OnFileSystemChange;
                Watcher.Deleted -= OnFileSystemChange;
                Watcher.Renamed -= OnFileSystemChange;
                Watcher.Dispose();
                Watcher = null;
            }
        }

        static async void OnFileSystemChange(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Contains("~") || e.Name.StartsWith("Content\\"))
            {
                return;
            }

            try
            {
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();
                var delay = Task.Delay(RestartDelay, TokenSource.Token);
                await delay;

                HttpRuntime.UnloadAppDomain();
            }
            catch { }
        }

    }
}