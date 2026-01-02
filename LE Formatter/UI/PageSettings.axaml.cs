using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LE_Formatter.lang;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LE_Formatter;

public partial class PageSettings : UserControl
{
    public static bool autoOpenLatest = true;

    public PageSettings()
    {
        InitializeComponent();
        DataContext = settingsBridge.Instance;

        foreach (settings.supportedLang l in Enum.GetValues(typeof(settings.supportedLang)))
        {
            ComboBoxItem cbi = new ComboBoxItem();
            cbi.Content = settings.langString(l);
            cbi.Tag = l;
            this.SettingComboBoxLanguage.Items.Add(cbi);

            if (settings.language == l) this.SettingComboBoxLanguage.SelectedItem = cbi;
        }
    }

    private async Task<string?> getFolderFromFileDialogue(string windowTitle, string startLocation = null)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        FolderPickerOpenOptions fpoo = new FolderPickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        };

        if (startLocation != null && startLocation.Length > 0)
        {
            fpoo.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(startLocation);
        }

        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(fpoo);
        if (folder != null && folder.Count > 0)
        {
            string path = folder.First().Path.LocalPath;
            if (Directory.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    private async void changeTs4FolderPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string? ret = await getFolderFromFileDialogue(Loc.GenericDialogueSelectFolder, startLocation: settings.theSimsDocumentsFolderPath);
        if (ret != null && settings.verifyAsTs4DocumentsFolder(ret, showMessages: true))
        {
            SettingTextBoxTs4FolderPath.Text = ret;
        }
    }

    private async void changeTs4InstallationPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string? ret = await getFolderFromFileDialogue(Loc.GenericDialogueSelectFolder, startLocation: settings.gameInstallFolderPath);
        if (ret != null && settings.verifyAsTs4InstallFolder(ret, showMessages: true))
        {
            SettingTextBoxTs4InstallationPath.Text = ret;
        }
    }

    private void onLanguageChange(object? sender, SelectionChangedEventArgs e)
    {
        settings.language = (settings.supportedLang) ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag;
    }
}