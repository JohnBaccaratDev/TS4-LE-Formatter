using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter.Localization
{
    public class ResxLocalizer : INotifyPropertyChanged
    {
        public static ResxLocalizer Instance { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private CultureInfo _culture = CultureInfo.CurrentUICulture;

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (Equals(_culture, value)) return;
                _culture = value;

                lang.Loc.Culture = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            }
        }

        public string this[string key] => lang.Loc.ResourceManager.GetString(key, _culture) ?? $"!{key}!";
    }
}
