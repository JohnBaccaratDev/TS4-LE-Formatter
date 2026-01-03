using Avalonia;
using Avalonia.Threading;
using LE_Formatter.UI;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;

namespace LE_Formatter
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
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

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
