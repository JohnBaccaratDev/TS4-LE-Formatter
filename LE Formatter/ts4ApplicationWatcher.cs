using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LE_Formatter
{
    public class ts4ApplicationWatcher
    {
        static int? id = null;

        public static System.Timers.Timer timer = null;

        public static void init()
        {
            if (timer != null) return;

            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(process);
            timer.Interval = 500;
            timer.Enabled = settings.autoOpenLatest;
            timer.Start();
        }

        public static void stop()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        private static void process(object? sender, ElapsedEventArgs e)
        {
            if (id != null)
            {
                try
                {
                    Process p = Process.GetProcessById((int)id);
                }
                catch
                {
                    id = null;
                }
            }
            else
            {
                Process[] p = Process.GetProcessesByName("TS4_x64");
                if(p.Length > 0 && !p.First().HasExited)
                {
                    id = p.First().Id;
                    Console.WriteLine(p.ToString());
                    if (Avalonia.Application.Current != null
                        && Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApplication
                        && desktopApplication.MainWindow is MainWindow mw)
                    {
                        Dispatcher.UIThread.Invoke(new Action(() => {
                            mw.LeFileTabs.Items.Clear();
                        }));
                    }
                }
            }
        }
    }
}
