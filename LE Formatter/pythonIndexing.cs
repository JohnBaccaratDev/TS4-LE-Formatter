using Avalonia.Controls.ApplicationLifetimes;
using Metsys.Bson;
using MsBox.Avalonia;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LE_Formatter
{

    public class pythonIndexing
    {

        public static indexEntry unknown = new indexEntry(Localization.ResxLocalizer.Instance["LeFileTabOriginUnknown"], indexEntry.indexType.zipFile, null);
        public static indexEntry vanillaGame = new indexEntry(Localization.ResxLocalizer.Instance["LeFileTabOriginTheSims4"], indexEntry.indexType.multipleZipFiles, null);
        public static List<indexEntry> mods = new List<indexEntry>();
        public static List<string> corruptZips = new List<string>();

        public static List<string> nonIndexablePycFiles = new List<string>();

        private static indexEntry? getModsIndexEntryByHash(string hash, string name, string path)
        {
            foreach (indexEntry ie in mods)
            {
                if (ie.hash.Equals(hash))
                {
                    ie.name = name;
                    ie.path = path;
                    return ie;
                }
            }
            return null;
        }

        static indexEntry? indexZip(string path, indexEntry ie=null)
        {
            byte[] md5 = MD5.HashData(File.ReadAllBytes(path));
            string hash = System.Text.Encoding.UTF8.GetString(md5, 0, md5.Length);
            string name = Path.GetFileName(path);
            if (ie == null)
            {
                ie = getModsIndexEntryByHash(hash, name, path);
                if (ie != null) return ie;

                if (ie == null) {
                    ie = new indexEntry(name, indexEntry.indexType.zipFile, hash, path);
                }
            }

            ZipArchive za;
            for(int i = 0; true; i++)
            {
                try
                {
                    za = ZipFile.OpenRead(path);
                    break;
                }
                catch (InvalidDataException ex)
                {
                    if(i < 10)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (!isFileWriteLocked(path))
                    {
                        if (!corruptZips.Contains(path))
                        {
                            bool hasAccomyingPartFile = false;
                            bool partFileChangedSizeOrLocked = false;

                            string zipFileName = Path.GetFileNameWithoutExtension(path);
                            foreach (string f in Directory.GetFiles(Path.GetDirectoryName(path), String.Format("{0}*", zipFileName)))
                            {
                                if (f.EndsWith(".part") || f.EndsWith(".crdownload"))
                                {
                                    hasAccomyingPartFile = true;
                                    if (!isFileWriteLocked(f))
                                    {
                                        partFileChangedSizeOrLocked = true;
                                    }
                                    else
                                    {
                                        long size = new System.IO.FileInfo(f).Length;

                                        for (int i2 = 0; i2 < 25; i++)
                                        {
                                            Thread.Sleep(20);

                                            if (size != new System.IO.FileInfo(f).Length)
                                            {
                                                partFileChangedSizeOrLocked = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (hasAccomyingPartFile != partFileChangedSizeOrLocked)
                            {
                                corruptZips.Add(path);
                                MessageBoxManager.GetMessageBoxStandard(
                                    lang.Loc.DialogueGeneralError,
                                    String.Format(lang.Loc.DialogueCorruptZip, path),
                                    MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                            }
                        }
                    }

                    return null;
                }
            }

            foreach(ZipArchiveEntry entry in za.Entries)
            {
                if (entry.FullName.EndsWith(".pyc"))
                {
                    string exceptionString = String.Format("{0} : {1}", path, entry.FullName);
                    if (nonIndexablePycFiles.Contains(exceptionString)) continue;

                    byte[] bytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        Stream e = entry.Open();
                        e.CopyTo(memoryStream);
                        bytes = memoryStream.ToArray();
                    }

                    try
                    {
                        ie.Add(pycGetCompiledFileName(bytes));
                    }
                    catch (Exception ex)
                    {
                        nonIndexablePycFiles.Add(exceptionString);
                        Program.logString(String.Format("The following compiled Python file could not be indexed \"{0}\"", exceptionString));
                        Program.logException(ex);
                    }
                }
            }

            return ie;
        }

        static indexEntry? indexScriptsFolder(string path, indexEntry ie = null)
        {
            string parentPath = Directory.GetParent(path).ToString();
            string[] files = Directory.GetFiles(path, "*.pyc", SearchOption.AllDirectories);
            if (files.Length < 1) return ie;

            List<byte> bytesOfFiles = new List<byte>();
            foreach (string file in files) {
                bytesOfFiles.AddRange(File.ReadAllBytes(file));
            }

            byte[] md5 = MD5.HashData(bytesOfFiles.ToArray());
            string hash = System.Text.Encoding.UTF8.GetString(md5, 0, md5.Length);
            string name = Directory.GetParent(path).Name;
            if (ie == null)
            {
                ie = getModsIndexEntryByHash(hash, name, parentPath);
                if (ie != null) return ie;

                ie = new indexEntry(name, indexEntry.indexType.scriptFolder, hash, parentPath);
            }

            foreach(string f in files)
            {
                if (nonIndexablePycFiles.Contains(f)) continue;

                try
                {
                    ie.Add(pycGetCompiledFileName(File.ReadAllBytes(f)));
                }
                catch (Exception ex)
                {
                    nonIndexablePycFiles.Add(f);
                    Program.logString(String.Format("The following compiled Python file could not be indexed \"{0}\"", f));
                    Program.logException(ex);
                }

            }

            return ie;
        }

        private static void startPython()
        {
            if (PythonEngine.IsInitialized) return;

            string baseDir = AppContext.BaseDirectory;
            string pythonHome;
            string pythonDll;

            pythonHome = Path.Combine(baseDir, "Python");
            pythonDll = Path.Combine(pythonHome, "python37.dll");

            Environment.SetEnvironmentVariable("PYTHONHOME", pythonHome);
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            PythonEngine.PythonHome = pythonHome;

            PythonEngine.Initialize();
        }

        private static void stopPython()
        {
            if(!PythonEngine.IsInitialized) return;

            // Note: This currently still needs the unsafe BinaryFormatter. It's globally enabled for the project currently in the .cjproj
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization",true);
            PythonEngine.Shutdown();
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);
        }
        public static bool isFileWriteLocked(string path)
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Write))
                {
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        private static void indexMods(bool takeCareOfPython = false)
        {
            startPython();

            List<indexEntry> newMods = new List<indexEntry>();

            if (Directory.Exists(settings.theSimsDocumentsFolderPath))
            {
                string modsFolder = Path.Combine(settings.theSimsDocumentsFolderPath, "Mods");
                indexModsRecurse(modsFolder, 0, newMods);
            }

            mods.Clear();
            newMods.Sort((x, y) => x.name.CompareTo(y.name));
            mods = newMods;

            if (takeCareOfPython) stopPython();
        }

        private static void indexModsRecurse(string path, int depth, List<indexEntry> newMods)
        {
            if (Directory.Exists(path))
            {
                string[] subPaths = Directory.GetDirectories(path);
                foreach (string sp in subPaths) {
                    if(depth < 1) indexModsRecurse(sp, depth + 1, newMods);

                    if (sp.EndsWith("Scripts"))
                    {
                        indexEntry ie = indexScriptsFolder(sp);
                        if (ie != null && ie.Count > 0) newMods.Add(ie);
                    }
                }

                foreach (string file in Directory.GetFiles(path, "*.zip"))
                {
                    indexEntry ie = indexZip(file);
                    if(ie != null && ie.Count > 0) newMods.Add(ie);
                }

                foreach (string file in Directory.GetFiles(path, "*.ts4script"))
                {
                    indexEntry ie = indexZip(file);
                    if (ie != null && ie.Count > 0) newMods.Add(ie);
                }

            }
        }

        private static void indexVanillaGame(bool takeCareOfPython=false)
        {
            startPython();

            if (Directory.Exists(settings.gameInstallFolderPath))
            {
                vanillaGame.Clear();
                string pythonZipFolder = Path.Combine(settings.gameInstallFolderPath, "Data", "Simulation", "Gameplay");
                if (Directory.Exists(pythonZipFolder))
                {
                    string[] files = Directory.GetFiles(pythonZipFolder, "*.zip");
                    foreach (string file in files)
                    {
                        indexZip(file, vanillaGame);
                    }
                }

                pythonZipFolder = Path.Combine(settings.gameInstallFolderPath, "Game", "Bin", "Python");
                if (Directory.Exists(pythonZipFolder))
                {
                    string[] files = Directory.GetFiles(pythonZipFolder, "*.zip");
                    foreach (string file in files)
                    {
                        indexZip(file, vanillaGame);
                    }
                }
            }

            if (takeCareOfPython) stopPython();
        }

        public static void startIndexing(bool preserveVanillaIndex=false, bool preserveModsIndex=false)
        {
            if (!settings.triedToReadSettingsFileAndAutoFill) return;

            if (!preserveVanillaIndex)
            {
                indexVanillaGame();
            }
            if (!preserveModsIndex)
            {
                indexMods();
            }

            generateIndexedScriptsPage();

            stopPython();

            PageLeFileTabContent.resetAssociations();

            // Otherwise, doesn't free a bunch of stuff for whatever reason on the first indexing for whatever reason
            GC.Collect();
        }

        public static void generateIndexedScriptsPage()
        {
            if (Avalonia.Application.Current != null
                && Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApplication 
                && desktopApplication.MainWindow is MainWindow mw)
            {
                ((PageIndexScripts) mw.TabIndexFiles.Content).indexStackPanel.Children.Clear();
                ((PageIndexScripts)mw.TabIndexFiles.Content).indexStackPanel.Children.Add(new IndexEntryControl(vanillaGame));
                foreach(indexEntry ie in mods)
                {
                    ((PageIndexScripts)mw.TabIndexFiles.Content).indexStackPanel.Children.Add(new IndexEntryControl(ie));
                }
            }
        }

        public static bool pathAffectsModIndexing(string path)
        {
            if (!Path.Exists(path)) return false;

            string modsPath = Path.Join(settings.theSimsDocumentsFolderPath, "Mods");
            if (!Directory.Exists(modsPath)) return false;


            string[] pathParts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            string[] modsParts = modsPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            // path has to at least be a subdirectory of modsPath
            if(pathParts.Length <= modsParts.Length) return false;
            int i = 0;
            for (; i < modsParts.Length; i++)
            {
                if(!pathParts[i].Equals(modsParts[i])) return false;
            }
            // Cut off the path at the end of "Mods"
            pathParts = pathParts.Skip(i - 1).ToArray();
            modsParts = modsParts.Skip(i - 1).ToArray();

            if(path.ToLower().EndsWith(".ts4script") || path.ToLower().EndsWith(".zip"))
            {
                if (pathParts.Length <= 2) return true;
            }
            else if(path.ToLower().EndsWith(".py") || path.ToLower().EndsWith(".pyc"))
            {
                for(int j =  0; j < pathParts.Length && j <= 2; j++)
                {
                    if (pathParts[j].ToLower().Equals("scripts")) return true;
                }
            }

            return false;
        }

        private static string? pycGetCompiledFileName(byte[] pyc)
        {
            if (!PythonEngine.IsInitialized) throw new Exception("PythonEngine is not initialized.");

            using (Py.GIL())
            {
                try
                {
                    dynamic mod = Py.Import("pyc_get_compiled_filename");
                    dynamic func = mod.pyc_get_compiled_filename;
                    dynamic result = func(pyc);
                    return (string)result.ToString();
                }
                catch (PythonException ex)
                {
                    throw new InvalidOperationException("Failed to read .pyc filename", ex);
                }
            }
        }
    }

    public class indexEntry : HashSet<string>
    {
        public enum indexType {
            scriptFolder,
            zipFile,
            multipleZipFiles
        }

        public IEnumerable<string> items => this;

        private string? _path;

        public string path
        {
            get => _path == null ? "" : _path.ToString();
            set
            {
                if (_path != value)
                {
                    _path = value;
                }
            }
        }

        public bool uiPathPartVisible
        {
            get => _path == null ? false : _path.Length <= 0 ? false : true;
        }

        public string name {  get; set; }
        public string headerText
        {
            get
            {
                return String.Format("{0} ({1} Scripts)", name, this.Count);
            }
        }
        public indexType type { get; set; }
        public string hash;

        public string itemsAsStringList
        {
            get
            {
                return String.Join('\n', this.items);
            }
        }

        public indexEntry(string n, indexType t, string h, string p = null)
        {
            name = n;
            type = t;
            hash = h;
            path = p;
        }

        public string typeText { 
            get {
                switch (type) {
                    case indexType.scriptFolder:
                        return lang.Loc.IndexFilesEntryTypeFolder;

                    case indexType.zipFile:
                        return lang.Loc.IndexFilesEntryTypeZipped;

                    case indexType.multipleZipFiles:
                        return lang.Loc.IndexFilesEntryTypeZippedMultiple;

                    default:
                        return lang.Loc.LeFileTabOriginUnknown;
                }        
            }
        }
    }
}
