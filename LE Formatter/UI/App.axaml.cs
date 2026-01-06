using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using I18N.Avalonia;
using MsBox.Avalonia;
using Python.Runtime;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LE_Formatter.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(); 
                desktop.MainWindow.Title = String.Format("LE Formatter {0} [{1}]"
                    , Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ,Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == "BuildDate").Value);
            }

            Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (sender, e) =>
            {
                Program.logException(e.Exception, threadInfo:"UI Thread");
                try
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueLoadLeGeneralError,
                        String.Format(lang.Loc.DialogueCodeException, "UI Thread", e.ToString()),
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();

                    e.Handled = true;
                }
                catch { }
            };

            base.OnFrameworkInitializationCompleted();
        }
    }
}