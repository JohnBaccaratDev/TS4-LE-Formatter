using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter.Localization
{
    public class LocalizationExt : MarkupExtension
    {
        public LocalizationExt() { }
        public LocalizationExt(string key) => Key = key;

        public string Key { get; set; } = "";

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Bind to ResxLocalizer.Instance[Key]
            return new Binding($"[{Key}]")
            {
                Source = ResxLocalizer.Instance,
                Mode = BindingMode.OneWay
            };
        }
    }
}
