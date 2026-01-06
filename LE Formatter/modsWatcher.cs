using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class modsWatcher
    {
        private static FileSystemWatcher watcher = null;
        private static List<string> toWatchList = new ();

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
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.IncludeSubdirectories = true;

            watcher.Created += redoIndexingAndCallstackAssociations;
            watcher.Changed += redoIndexingAndCallstackAssociations;
            watcher.Renamed += redoIndexingAndCallstackAssociations;
            watcher.Deleted += redoIndexingAndCallstackAssociations;

            watcher.EnableRaisingEvents = settings.autoOpenLatest;
        }

        private static List<string> getToWatchList()
        {
            List<string> ret = new List<string>();

            ret.AddRange(Directory.GetFiles(watcher.Path, "*.pyc", SearchOption.AllDirectories));
            ret.AddRange(Directory.GetFiles(watcher.Path, "*.py", SearchOption.AllDirectories));
            ret.AddRange(Directory.GetFiles(watcher.Path, "*.ts4script", SearchOption.AllDirectories));
            ret.AddRange(Directory.GetFiles(watcher.Path, "*.zip", SearchOption.AllDirectories));

            return ret;
        }

        private static bool watchListDifferentFromCurrent(List<string> newList)
        {
            if (newList.Count != toWatchList.Count) return true;

            foreach(string x in toWatchList)
            {
                bool contains = false;
                foreach(string y in newList)
                {
                    if (x.Equals(y))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool changedFileToIndex(FileSystemEventArgs e)
        {
            if(e.ChangeType == WatcherChangeTypes.Changed || 
                e.ChangeType == WatcherChangeTypes.Created ||
                e.ChangeType == WatcherChangeTypes.Deleted)
            {
                if (e.FullPath.ToLower().EndsWith(".pyc")) return true;
                if (e.FullPath.ToLower().EndsWith(".py")) return true;
                if (e.FullPath.ToLower().EndsWith(".zip")) return true;
                if (e.FullPath.ToLower().EndsWith(".ts4script")) return true;
            }
            return false;
        }

        public static void redoIndexingAndCallstackAssociations(object sender, FileSystemEventArgs e)
        {
            List<string> newWatchList;

            newWatchList = getToWatchList();

            if (watchListDifferentFromCurrent(newWatchList) || changedFileToIndex(e))
            {
                mcccReportWatcher.enableIfNecessary();

                Dispatcher.UIThread.Invoke(new Action(() => {
                    pythonIndexing.startIndexing();
                }));
            }
            toWatchList = newWatchList;
        }
    }
}
