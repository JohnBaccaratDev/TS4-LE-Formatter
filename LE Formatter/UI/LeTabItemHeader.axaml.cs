using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LE_Formatter;

public partial class LeTabItemHeader : UserControl
{
    public string title
    {
        get; set;
    }
    public LeTabItemHeader(string text)
    {
        title = text.Trim();
        InitializeComponent();
        DataContext = this;
    }

    private void removeTab(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((TabControl)((TabItem)this.Parent).Parent).Items.Remove((TabItem)this.Parent);
    }
}