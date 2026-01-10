using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LE_Formatter
{
    public class mcccReportWatcher
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
            enableIfNecessary();
        }

        public static bool isEnabled => (watcher != null && watcher.EnableRaisingEvents);

        public static void enableIfNecessary()
        {
            if (watcher == null) return;

            string[] mccc = Directory.GetFiles(watcher.Path, "mc_cmd_center.ts4script", SearchOption.AllDirectories);
            string[] be = Directory.GetFiles(watcher.Path, "Tmex-BetterExceptions.ts4script", SearchOption.AllDirectories);

            watcher.EnableRaisingEvents = (mccc.Length > 0 && be.Length == 0);
        }

        public static void init()
        {
            if (watcher != null || settings.theSimsDocumentsFolderPath == null) return;

            watcher = new FileSystemWatcher(settings.theSimsDocumentsFolderPath);

            watcher.Filter = "mc_lastexception.html";
            watcher.IncludeSubdirectories = true;

            watcher.Created += onCreateOrChange;
            watcher.Changed += onCreateOrChange;

            enableIfNecessary();
        }
        public static void stop()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
        }

        public async static Task loadMcccReport(string path, bool fromAutoOpen = false)
        {
            XmlDocument x = new XmlDocument();
            bool loaded = false;
            int count = 0;
            int maxCount = 400;

            while (!loaded)
            {
                string lastExMessage = "";
                try
                {
                    x.Load(path);
                    loaded = true;
                }
                catch (XmlException ex)
                {
                    lastExMessage = ex.Message;
                    if (fromAutoOpen)
                    {
                        await MessageBoxManager.GetMessageBoxStandard(
                            lang.Loc.DialogueLoadLeGeneralError,
                            String.Format(lang.Loc.DialogueLoadLeNotXML, path, ex.Message),
                            MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastExMessage = ex.Message;
                }

                if (!loaded)
                {
                    Thread.Sleep(5);
                    count++;
                    if (count > maxCount)
                    {
                        if (fromAutoOpen)
                        {
                            await MessageBoxManager.GetMessageBoxStandard(
                                lang.Loc.DialogueLoadLeGeneralError,
                                String.Format(lang.Loc.DialogueLoadLeCouldNotLoad, path, lastExMessage),
                                MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                        }
                        return;
                    }
                }
            }

            XmlNodeList xnl = x.SelectNodes("/html/body/div");

            XmlNode currSummary = null;
            XmlNode currCallstack = null;
            string title = "";

            if (xnl != null && xnl.Count > 0)
            {
                foreach (XmlNode xn in xnl)
                {
                    if(currSummary == null)
                    {
                        foreach(XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name.Equals("class") && attr.Value.ToLower().Split(" ").Any(x => x.Equals("summarydiv")))
                            {
                                currSummary = xn;
                            }
                        }
                    }
                    else if (currCallstack == null)
                    {
                        bool classRight = false;
                        bool idRight = false;
                        foreach (XmlAttribute attr in xn.Attributes)
                        {
                            if (attr.Name.Equals("class") && attr.Value.ToLower().Split(" ").Any(x => x.Equals("infodiv")))
                            {
                                classRight = true;
                            }

                            if (attr.Name.Equals("id") && attr.Value.ToLower().Split(" ").Any(x => x.StartsWith("callstack")))
                            {
                                idRight = true;
                                title = attr.Value;
                            }
                        }

                        if (classRight && idRight)
                        {
                            currCallstack = xn;
                        }
                    }

                    if (currSummary != null && currCallstack != null)
                    {
                        string scriptException = null;
                        XmlNodeList summaryLines = currSummary.SelectNodes("ul/li");
                        foreach(XmlNode xn2 in summaryLines)
                        {
                            if (xn2.InnerText.Contains("Error message:"))
                            {
                                scriptException = xn2.InnerText;
                                scriptException = scriptException.Substring(scriptException.IndexOf("Error message:") + "Error message:".Length);

                                if (scriptException.Contains(", CategoryID"))
                                {
                                    scriptException = scriptException.Substring(0, scriptException.IndexOf(", CategoryID"));
                                }
                                scriptException = scriptException.Trim();

                                while (scriptException.StartsWith('(') && scriptException.EndsWith(')'))
                                {
                                    scriptException = scriptException.Substring(1, scriptException.Length - 2);
                                }
                                scriptException = scriptException.Trim();

                            }
                        }

                        if (scriptException == null) scriptException = lang.Loc.LeFileTabMcccNoScriptExceptionMessage;


                        XmlNodeList callStackLines = currCallstack.SelectNodes("ul/li");
                        List<string> callstack = new List<string>();
                        foreach(XmlNode xn2 in callStackLines)
                        {
                            string str = xn2.InnerText;

                            // Note: Trying to just remove [bla[bla]bla] doesn't work, as MCCC cuts off the appended string and adds a single ] at the end
                            // Meaning you will run into situations where there are an unequal number of '[' and ']'
                            // So, just cut it off at the first one
                            if (str.Contains('['))
                            {
                                str = str.Substring(0, str.IndexOf('['));
                            }

                            while (str.StartsWith('\n') || str.StartsWith('\r') || str.StartsWith('\t'))
                            {
                                str = str.Substring(1);
                            }

                            str = str.Trim();

                            callstack.Add(str);
                        }

                        string inGameErrorMessage = lang.Loc.LeFileTabMcccWarning;
                        callstack = callstack.Where(x => x.Trim().Length > 0).ToList();

                        if (callstack.Count > 0 && scriptException != null) {


                            int hash = (inGameErrorMessage.Trim() + string.Join("", callstack) + scriptException.Trim()).GetHashCode();
                            if (Avalonia.Application.Current != null && Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApplication && desktopApplication.MainWindow is MainWindow mw)
                            {
                                if (!mw.selectTabFromHash(hash))
                                {
                                    mw.addLeTab(hash, String.Format("[MCCC] {0}", title), inGameErrorMessage, callstack, scriptException);
                                }
                            }
                        }

                        currSummary = null;
                        currCallstack = null;
                    }
                }
            }
        }

        private async static void onCreateOrChange(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath)) return;

            Dispatcher.UIThread.Invoke(() =>
            {
                loadMcccReport(e.FullPath);
            });
        }
    }
}
