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
    public class modsWatcher
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
            if (watcher != null || settings.theSimsDocumentsFolderPath == null) return;

            watcher = new FileSystemWatcher(Path.Join(settings.theSimsDocumentsFolderPath));

            watcher.Filters.Add("*.pyc");
            watcher.Filters.Add("*.py");
            watcher.Filters.Add("*.ts4script");
            watcher.Filters.Add("*.zip");

            watcher.IncludeSubdirectories = true;

            watcher.Created += redoIndexingAndCallstackAssociations;
            watcher.Changed += redoIndexingAndCallstackAssociations;
            watcher.Renamed += redoIndexingAndCallstackAssociations;
            watcher.Deleted += redoIndexingAndCallstackAssociations;

            watcher.EnableRaisingEvents = settings.autoOpenLatest;
        }

        public static void redoIndexingAndCallstackAssociations(object sender, FileSystemEventArgs e)
        {
            mcccReportWatcher.enableIfNecessary();

            Dispatcher.UIThread.Invoke(new Action(() => {
                pythonIndexing.startIndexing();
            }));
        }
    }
}
