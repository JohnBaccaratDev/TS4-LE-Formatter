using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DialogHostAvalonia.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace LE_Formatter;

public partial class PageLeFileTabContent : UserControl
{
    public static readonly StyledProperty<string?> _path = AvaloniaProperty.Register<PageLeFileTabContent, string?>(nameof(path));

    public ObservableCollection<LeCallStackEntry> CallStack { get; } = new();

    public int LeHash;
    public string? path {
        get => GetValue(_path);
        set => SetValue(_path, value);
    }

    private ulong? lastClickedTimestamp = null;

    public PageLeFileTabContent()
    {
        InitializeComponent();
        DataContext = this;
    }

    public static void resetAssociations()
    {
        if (Avalonia.Application.Current != null && Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApplication && desktopApplication.MainWindow is MainWindow mw)
        {
            foreach (TabItem tab in mw.LeFileTabs.Items)
            {
                foreach (LeCallStackEntry cs in ((PageLeFileTabContent)tab.Content).CallStack)
                {
                    cs.setAssociatedOrigins();
                }
            }
        }

    }

    private void onOriginClicked(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed)
        {
            foreach (indexEntry ie in ((LeCallStackEntry)((DataGridCell)sender).DataContext).associatedOrigins)
            {
                util.openExplorerWithSelected(ie.path);
            }
        }
    }

    private void cellDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        foreach (LeCallStackEntry lcse in ((DataGrid)sender).SelectedItems)
        {
            foreach (indexEntry ie in lcse.associatedOrigins)
            {
                util.openExplorerWithSelected(ie.path);
            }
        }
    }

    private void openWikiQnA(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        util.openBrowserOnPage("https://github.com/JohnBaccaratDev/TS4-LE-Formatter/wiki/Question-&-Answers");
    }

    private void openLePath(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed)
        {
            util.openExplorerWithSelected(this.path.ToString());
        }
    }
}