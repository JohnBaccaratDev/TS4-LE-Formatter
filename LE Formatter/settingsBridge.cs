using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class settingsBridge : INotifyPropertyChanged
    {
        public static settingsBridge Instance { get; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool autoOpenLatest
        {
            get => settings.autoOpenLatest;
            set {
                if (value != settings.autoOpenLatest)
                {
                    settings.autoOpenLatest = value;
                    PropertyChanged?.Invoke(this, new(nameof(autoOpenLatest)));
                }
            }
        }

        public bool autoOpenLatestBringToFront
        {
            get => settings.autoOpenLatestBringToFront;
            set
            {
                if (value != settings.autoOpenLatestBringToFront)
                {
                    settings.autoOpenLatestBringToFront = value;
                    PropertyChanged?.Invoke(this, new(nameof(autoOpenLatestBringToFront)));
                }
            }
        }

        public bool autoReIndex
        {
            get => settings.autoReIndex;
            set
            {
                if (value != settings.autoReIndex)
                {
                    settings.autoReIndex = value;
                    PropertyChanged?.Invoke(this, new(nameof(autoReIndex)));
                }
            }
        }

        public string gameInstallFolderPath
        {
            get => settings.gameInstallFolderPath == null ? "" : settings.gameInstallFolderPath;
            set
            {
                if (value != settings.gameInstallFolderPath && Path.Exists(value))
                {
                    settings.gameInstallFolderPath = value;
                    PropertyChanged?.Invoke(this, new(nameof(gameInstallFolderPath)));
                }
            }
        }

        public string theSimsDocumentsFolderPath
        {
            get => settings.theSimsDocumentsFolderPath == null ? "" : settings.theSimsDocumentsFolderPath;
            set
            {
                if (value != settings.theSimsDocumentsFolderPath && Path.Exists(value))
                {
                    settings.theSimsDocumentsFolderPath = value;
                    PropertyChanged?.Invoke(this, new(nameof(theSimsDocumentsFolderPath)));
                }
            }
        }
    }
}
