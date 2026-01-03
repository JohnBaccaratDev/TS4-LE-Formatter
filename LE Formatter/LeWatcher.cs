using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class LeWatcher
    {
        private static FileSystemWatcher watcher = null;

        public static void setWatchPath(string path)
        {
            if (watcher == null) init();
            if (watcher == null) return;

            if (Directory.Exists(path))
            {
                watcher.Path = path;
            }
        }

        public static void setEnableEvents(bool enabled)
        {
            if (watcher == null) return;

            watcher.EnableRaisingEvents = enabled;
        }

        public static void init()
        {
            if (watcher != null || settings.theSimsDocumentsFolderPath == null ) return;

            watcher = new FileSystemWatcher(settings.theSimsDocumentsFolderPath);

            watcher.Filter = "lastException*.txt";

            watcher.Created += onCreateOrChange;
            watcher.Changed += onCreateOrChange;

            watcher.EnableRaisingEvents = settings.autoOpenLatest;
        }

        public static void stop()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
        }

        private async static void onCreateOrChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath)) return;

            if(Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApplication && desktopApplication.MainWindow is MainWindow mw)
            {
                Dispatcher.UIThread.Invoke(new Action(() => { 
                    mw.loadLeFile(e.FullPath, fromAutoOpen:true);
                }));
            }
        }
    }
}
