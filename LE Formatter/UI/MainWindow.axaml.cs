using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LE_Formatter
{
    public partial class MainWindow : Window
    {
        public static bool labelDragAndDropVisible
        {
            get => !settings.autoOpenLatest;
        }
        public static bool labelDragAndDropAutoVisible
        {
            get => settings.autoOpenLatest;
        }

        public MainWindow()
        {
            InitializeComponent();

            Opened += OnWindowOpened;
        }

        private void OnWindowOpened(object? sender, System.EventArgs e)
        {
            // Set the margin of ItemsPresenter of LeTabs manually, since doing it through style and resources is less readable.
            this.LeFileTabs.Items.CollectionChanged += onLeTabsChanged;

            pythonIndexing.generateIndexedScriptsPage();
            processStartupSettingErrors();
        }

        private void fixLeTabThickness()
        {
            this.LeFileTabs.IsVisible = this.LeFileTabs.Items.Count != 0;
            if (this.LeFileTabs.IsVisible)
            {
                ItemsPresenter cp = (ItemsPresenter)this.LeFileTabs.GetVisualDescendants().FirstOrDefault(p => p.Name == "PART_ItemsPresenter");
                if (cp != null)
                {
                    cp.Margin = new Avalonia.Thickness(0);
                }
            }
        }

        private void DragAndDrop(object? sender, Avalonia.Input.DragEventArgs e)
        {
            foreach (var f in e.DataTransfer.TryGetFiles())
            {
                string fileName = Path.GetFileName(f.Path.AbsolutePath);

                if (fileName.StartsWith("mc_") && fileName.EndsWith(".html"))
                {
                    mcccReportWatcher.loadMcccReport(f.Path.AbsolutePath.ToString());
                }
                else
                {
                    loadLeFile(f.Path.AbsolutePath.ToString());
                }
            }
        }

        public void processStartupSettingErrors()
        {
            while (settings.startupErrorList.Count > 0) {
                switch (settings.startupErrorList.Pop())
                {
                    case settings.startupErrors:
                        MessageBoxManager.GetMessageBoxStandard(
                            lang.Loc.DialogueSettingsGeneralError,
                            lang.Loc.DialogueSettingsPathsNull,
                            MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                        break;
                }
            }
        }

        public bool selectTabFromHash(int hash)
        {
            foreach (TabItem tab in this.LeFileTabs.Items)
            {
                if (((PageLeFileTabContent)tab.Content).LeHash == hash)
                {
                    this.LeFileTabs.SelectedItem = tab;
                    return true;
                }
            }
            return false;
        }

        public async Task loadLeFile(string path, bool fromAutoOpen = false)
        {
            XmlDocument x = new XmlDocument();
            bool loaded = false;
            int count = 0;
            int maxCount = 20;
            if (fromAutoOpen)
            {
                maxCount = 400;
            }

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
                    if (!fromAutoOpen)
                    {
                        await MessageBoxManager.GetMessageBoxStandard(
                            lang.Loc.DialogueLoadLeGeneralError,
                            String.Format(lang.Loc.DialogueLoadLeNotXML, path, ex.Message),
                            MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                        return;
                    }
                }
                catch(Exception ex)
                {
                    lastExMessage = ex.Message;
                }

                if (!loaded)
                {
                    Thread.Sleep(5);
                    count++;
                    if (count > maxCount)
                    {
                        if (!fromAutoOpen)
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
            DateTime dt = System.IO.File.GetLastWriteTime(path);

            XmlNodeList xnl = x.SelectNodes("/root/report/desyncdata");
            if (xnl != null && xnl.Count > 0)
            {
                foreach (XmlNode descyncdata in xnl)
                {
                    List<string> s = descyncdata.InnerText.Split("\r\n").ToList();

                    // Throw out empty lines
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (s[i].Trim().Length == 0)
                        {
                            s.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    string inGameErrorMessage = "";
                    // Get inGameErrorMessage
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (s[i].Equals("Traceback (most recent call last):"))
                        {
                            s.RemoveAt(i);
                            i--;
                            for (int j = i; j >= 0; j--)
                            {
                                inGameErrorMessage = s[j].Trim() + (inGameErrorMessage.Length != 0 ? '\n' : "") + inGameErrorMessage;

                                s.RemoveAt(j);
                            }
                            break;
                        }
                    }
                    inGameErrorMessage = inGameErrorMessage.Replace("[manus] ", "");


                    // Merge lines starting with 4 spaces with the related previous
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (s[i].Trim().Length == 0)
                        {
                            s.RemoveAt(i);
                            i--;
                        }
                        else if (s[i].StartsWith("    ") && i > 0)
                        {
                            s[i - 1] += '\n' + s[i];
                            s.RemoveAt(i);
                            i--;
                        }
                    }

                    string scriptException = "";
                    bool encounteredRtim = false;
                    // Get scriptException
                    for (int i = s.Count - 1; i >= 0; i--)
                    {

                        if (s[i].Trim().EndsWith("rtim=0"))
                        {
                            encounteredRtim = true;
                            scriptException = s[i].Trim().Substring(0, s[i].Length - 6);

                            s.RemoveAt(i);
                            continue;
                        }

                        if (s[i].StartsWith("  File"))
                        {
                            break;
                        }

                        if (encounteredRtim)
                        {
                            scriptException = s[i].Trim() + '\n' + scriptException;
                        }
                        s.RemoveAt(i);
                    }
                    // We now just have the actual call stack.

                    // Skip duplicates
                    int hash = (inGameErrorMessage.Trim() + string.Join("", s) + scriptException.Trim()).GetHashCode();
                    if (selectTabFromHash(hash)) continue;

                    string header = inGameErrorMessage.PadRight(20).Substring(0, 20) + "... " + dt.ToString("HH:mm:ss");
                    addLeTab(hash, header, inGameErrorMessage, s, scriptException);

                    if (fromAutoOpen && settings.autoOpenLatestBringToFront)
                    {
                        this.Activate();
                    }
                }
            }
            else
            {
                if (!fromAutoOpen)
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueLoadLeGeneralError, 
                        String.Format(lang.Loc.DialogueLoadLeContainsNoLEInside, path),
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
            }
        }

        public void addLeTab(int hash, string header, string inGameErrorMessage, List<string> stringCallStack, string scriptException)
        {
            PageLeFileTabContent le = new PageLeFileTabContent();
            le.LeTextBlockErrorMessage.Text = inGameErrorMessage;
            le.LeTextBlockExceptionMessage.Text = scriptException;
            le.LeHash = hash;

            List<LeCallStackEntry> cs = new List<LeCallStackEntry>();
            for (int i = 0; i < stringCallStack.Count; i++)
            {
                string s = stringCallStack[i];
                s = s.Substring(s.IndexOf("File ") + "File ".Length);
                while(s.StartsWith('\'') || s.StartsWith('\"'))
                {
                    s = s.Substring(1);
                }

                string file = s.Substring(0, s.IndexOf(", line "));
                while (file.EndsWith('\'') || file.EndsWith('\"'))
                {
                    file = file.Substring(0, file.Length - 1);
                }

                s = s.Substring(s.IndexOf(", line ") + ", line ".Length);

                string sLine = s.Substring(0, s.IndexOf(','));
                s = s.Remove(0, sLine.Length + ", in ".Length);

                le.CallStack.Add(new LeCallStackEntry(file, sLine, s));
            }

            TabItem tab = new TabItem();
            tab.Header = new LeTabItemHeader(header);

            tab.Content = le;

            this.LeFileTabs.Items.Add(tab);

            tab.PointerPressed += tabPointerPressed;
            this.LeFileTabs.SelectedItem = tab;

            MainTabs.SelectedItem = TabLeViewer;
        }

        private void onLeTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            fixLeTabThickness();
        }

        private void TabLeViewer_LayoutUpdated(object? sender, EventArgs e)
        {
            fixLeTabThickness();
        }

        private void tabPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Properties.IsMiddleButtonPressed)
            {
                ((TabControl)((TabItem)sender).Parent).Items.Remove((TabItem)sender);
            }
        }

        private void buttonClickedLoadRecentLes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            foreach (string f in Directory.EnumerateFiles(settings.theSimsDocumentsFolderPath, "lastException*.txt"))
            {
                loadLeFile(f);
            }

            if (mcccReportWatcher.isEnabled)
            {
                foreach (string f in Directory.EnumerateFiles(Path.Join(settings.theSimsDocumentsFolderPath, "Mods"), "mc_lastexception.html", SearchOption.AllDirectories))
                {
                    mcccReportWatcher.loadMcccReport(f);
                }
            }
        }

        private async void buttonClickedClearLes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            bool error = false;

            foreach(string f in Directory.EnumerateFiles(settings.theSimsDocumentsFolderPath, "lastException*.txt"))
            {
                try
                {
                    File.Delete(f);
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
            {
                Task<ButtonResult> br = MessageBoxManager.GetMessageBoxStandard(
                    lang.Loc.DialogueLoadLeGeneralError,
                    String.Format(lang.Loc.DialogueLoadLeContainsNoLEInside),
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync();

                if(br.Result == ButtonResult.Yes)
                {
                    util.openExplorerOnPath(settings.theSimsDocumentsFolderPath);
                }
            }

            if (mcccReportWatcher.isEnabled)
            {
                foreach (string f in Directory.EnumerateFiles(Path.Join(settings.theSimsDocumentsFolderPath, "Mods"), "mc_lastexception.html", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}