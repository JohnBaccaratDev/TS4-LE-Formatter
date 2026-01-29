using Avalonia;
using Avalonia.Threading;
using LE_Formatter.UI;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LE_Formatter {

    internal class Program
    {
        private static string logFile = "LE Formatter Log.txt";

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            try
            {
                File.Delete(logFile);
            }
            catch {}

            try
            {
                (bool settingFileExists, List<string> filledItems) filledSettings = settings.readSettingsFile();
                settings.tryFillSettings(filledSettings.filledItems);
                settings.triedToReadSettingsFileAndAutoFill = true;
                if (!filledSettings.settingFileExists)
                {
                    if (settings.gameInstallFolderPath == null || settings.theSimsDocumentsFolderPath == null)
                    {
                        settings.startupErrorList.Push(settings.startupErrors.pathsNull);
                    }
                }

                pythonIndexing.startIndexing();
                LeWatcher.init();
                modsWatcher.init();
                mcccReportWatcher.init();
                ts4ApplicationWatcher.init();

                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    logException((Exception)e.ExceptionObject, threadInfo: "AppDomain.CurrentDomain.UnhandledException");
                };
                
                TaskScheduler.UnobservedTaskException += (sender, e) =>
                {
                    logException(e.Exception, threadInfo: "TaskScheduler.UnobservedTaskException");
                    e.SetObserved();
                };

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }catch(Exception ex)
            {
                logException(ex, threadInfo:"Program.Main");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();

        public static void logString(string message)
        {
            using (var fw = File.AppendText(logFile))
            {
                fw.WriteLine(DateTime.Now.ToString());
                fw.WriteLine(message);
                fw.WriteLine();
            }
        }

        public static void logException(Exception ex, string threadInfo=null)
        {
            using(var fw = File.AppendText(logFile))
            {
                fw.WriteLine(DateTime.Now.ToString());
                fw.WriteLine("------------------------------");
                if (threadInfo != null)
                {
                    fw.WriteLine(String.Format("Thread: {0}", threadInfo));
                }
                fw.WriteLine("Exception:");
                fw.WriteLine(ex.ToString());
                fw.WriteLine();
                fw.WriteLine();
            }
        }
    }
}
