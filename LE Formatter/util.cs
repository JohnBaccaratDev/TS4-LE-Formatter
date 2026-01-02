using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class util
    {

        public static void openBrowserOnPage(string url)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = url;
            info.UseShellExecute = true;
            Process.Start(info);
        }
        public static bool openExplorerOnPath(string path)
        {
            if (Directory.Exists(path))
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "explorer";
                info.Arguments = string.Format("\"{0}\"", path);
                Process.Start(info);
                return false;
            }
            return false;
        }
        public static bool openExplorerWithSelected(string path)
        {
            if (File.Exists(path))
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "explorer";
                info.Arguments = string.Format("/e, /select, \"{0}\"", path);
                Process.Start(info);
                return false;
            }
            return false;
        }
    }
}
