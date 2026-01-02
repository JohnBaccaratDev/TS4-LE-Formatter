using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using System.IO;

namespace LE_Formatter;

public partial class IndexEntryControl : UserControl
{
    public IndexEntryControl(indexEntry ie)
    {
        InitializeComponent();
        DataContext = ie;
    }

    private void openPath(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        util.openExplorerWithSelected(((indexEntry)this.DataContext).path);
    }

    private void openPath(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        string p = ((indexEntry)this.DataContext).path;
        if (File.Exists(p))
        {
            string[] args = { "/e", "/select", p };
            Process.Start("explorer.exe", args);
        }
    }
}