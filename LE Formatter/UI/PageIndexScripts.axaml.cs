using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace LE_Formatter;

public partial class PageIndexScripts : UserControl
{
    public PageIndexScripts()
    {
        InitializeComponent();
    }

    private void reIndexFiles(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        indexStackPanel.Children.Clear();
        Dispatcher.UIThread.Post(new Action(() =>
        {
            pythonIndexing.startIndexing(preserveVanillaIndex: true);
        }));
    }
}