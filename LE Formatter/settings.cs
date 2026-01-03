using Avalonia;
using MicroCom.Runtime;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class settings
    {
        public static bool triedToReadSettingsFileAndAutoFill = false;

        public enum startupErrors
        {
            pathsNull
        }

        public static Stack<startupErrors> startupErrorList = new Stack<startupErrors>();

        public enum supportedLang
        {
            en_US,
            zh_CN,
            zh_TW,
            cs_CZ,
            da_DK,
            de_AT, // A.E.I.O.U., Oida.
            nl_NL,
            fi_FI,
            fr_FR,
            it_IT,
            ja_JP,
            ko_KR,
            nb_NO,
            pl_PL,
            ru_RU,
            es_ES,
            sv_SE,
            pt_BR,
        }
        public static string langString(supportedLang l)
        {
            switch (l)
            {
                case supportedLang.en_US: return lang.Loc.GenericLanguage_en_US;
                case supportedLang.de_AT: return lang.Loc.GenericLanguage_de_AT;
                case supportedLang.zh_CN: return lang.Loc.GenericLanguage_zh_CN;
                case supportedLang.zh_TW: return lang.Loc.GenericLanguage_zh_TW;
                case supportedLang.cs_CZ: return lang.Loc.GenericLanguage_cs_CZ;
                case supportedLang.da_DK: return lang.Loc.GenericLanguage_da_DK;
                case supportedLang.nl_NL: return lang.Loc.GenericLanguage_nl_NL;
                case supportedLang.fi_FI: return lang.Loc.GenericLanguage_fi_FI;
                case supportedLang.fr_FR: return lang.Loc.GenericLanguage_fr_FR;
                case supportedLang.it_IT: return lang.Loc.GenericLanguage_it_IT;
                case supportedLang.ja_JP: return lang.Loc.GenericLanguage_ja_JP;
                case supportedLang.ko_KR: return lang.Loc.GenericLanguage_ko_KR;
                case supportedLang.nb_NO: return lang.Loc.GenericLanguage_nb_NO;
                case supportedLang.pl_PL: return lang.Loc.GenericLanguage_pl_PL;
                case supportedLang.ru_RU: return lang.Loc.GenericLanguage_ru_RU;
                case supportedLang.es_ES: return lang.Loc.GenericLanguage_es_ES;
                case supportedLang.sv_SE: return lang.Loc.GenericLanguage_sv_SE;
                case supportedLang.pt_BR: return lang.Loc.GenericLanguage_pt_BR;
            }
            return "";
        }
        private static supportedLang _language = supportedLang.en_US;
        public static supportedLang language
        {
            get => _language;
            set
            {
                if (value != _language)
                {
                    _language = value;
                    setLanguage(value);
                    writeSettingsFile();
                }
            }
        }
        private static void setLanguage(supportedLang l)
        {
            CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            switch (l)
            {
                case supportedLang.de_AT: ci = new System.Globalization.CultureInfo("de-AT"); break;
                case supportedLang.zh_CN: ci = new System.Globalization.CultureInfo("zh-CN"); break;
                case supportedLang.zh_TW: ci = new System.Globalization.CultureInfo("zh-TW"); break;
                case supportedLang.cs_CZ: ci = new System.Globalization.CultureInfo("cs-CZ"); break;
                case supportedLang.da_DK: ci = new System.Globalization.CultureInfo("da-DK"); break;
                case supportedLang.nl_NL: ci = new System.Globalization.CultureInfo("nl-NL"); break;
                case supportedLang.fi_FI: ci = new System.Globalization.CultureInfo("fi-FI"); break;
                case supportedLang.fr_FR: ci = new System.Globalization.CultureInfo("fr-FR"); break;
                case supportedLang.it_IT: ci = new System.Globalization.CultureInfo("it-IT"); break;
                case supportedLang.ja_JP: ci = new System.Globalization.CultureInfo("ja-JP"); break;
                case supportedLang.ko_KR: ci = new System.Globalization.CultureInfo("ko-KR"); break;
                case supportedLang.nb_NO: ci = new System.Globalization.CultureInfo("nb-NO"); break;
                case supportedLang.pl_PL: ci = new System.Globalization.CultureInfo("pl-PL"); break;
                case supportedLang.ru_RU: ci = new System.Globalization.CultureInfo("ru-RU"); break;
                case supportedLang.es_ES: ci = new System.Globalization.CultureInfo("es-ES"); break;
                case supportedLang.sv_SE: ci = new System.Globalization.CultureInfo("sv-SE"); break;
                case supportedLang.pt_BR: ci = new System.Globalization.CultureInfo("pt-BR"); break;
            }

            Localization.ResxLocalizer.Instance.Culture = ci;

            pythonIndexing.unknown.name = lang.Loc.LeFileTabOriginUnknown;
            pythonIndexing.vanillaGame.name = lang.Loc.LeFileTabOriginTheSims4;
        }

        private static bool _autoOpenLatest = true;
        public static bool autoOpenLatest
        {
            get => _autoOpenLatest;
            set
            {
                if (value != _autoOpenLatest)
                {
                    _autoOpenLatest = value;

                    LeWatcher.setEnableEvents(value);
                    writeSettingsFile();
                }
            }
        }

        private static bool _autoOpenLatestBringToFront = true;
        public static bool autoOpenLatestBringToFront
        {
            get => _autoOpenLatestBringToFront;
            set
            {
                if (value != _autoOpenLatestBringToFront)
                {
                    _autoOpenLatestBringToFront = value;

                    writeSettingsFile();
                }
            }
        }

        private static bool _autoReIndex = true;
        public static bool autoReIndex
        {
            get => _autoReIndex;
            set
            {
                if (value != _autoReIndex)
                {
                    _autoReIndex = value;

                    modsWatcher.setEnableEvents(value);
                    writeSettingsFile();
                }
            }
        }

        private static string _theSimsDocumentsFolderPath = null;
        public static string theSimsDocumentsFolderPath {
            get => _theSimsDocumentsFolderPath;
            set {
                if (value != _theSimsDocumentsFolderPath)
                {
                    _theSimsDocumentsFolderPath = value;

                    LeWatcher.setWatchPath(value);
                    modsWatcher.setWatchPath(Path.Join(value, "Mods"));
                    mcccReportWatcher.setWatchPath(Path.Join(value, "Mods"));
                    pythonIndexing.startIndexing(preserveVanillaIndex:true);
                    PageLeFileTabContent.resetAssociations();
                    writeSettingsFile();
                }
            }
        }

        private static string _gameInstallFolderPath = null;
        public static string gameInstallFolderPath
        {
            get => _gameInstallFolderPath;
            set
            {
                if (value != _gameInstallFolderPath)
                {
                    _gameInstallFolderPath = value;

                    pythonIndexing.startIndexing(preserveModsIndex:true);
                    PageLeFileTabContent.resetAssociations();
                    writeSettingsFile();
                }
            }
        }

        public static float fontScale = 1f;

        private static string getSettingsFileSeperator()
        {
            return "=:=";
        }

        public static string getSettingsFilePath()
        {
            return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "LE-Formatter Settings." + Environment.UserName + ".ini");
        }

        public static void writeSettingsFile()
        {
            if (!triedToReadSettingsFileAndAutoFill) return;

            File.WriteAllLines(getSettingsFilePath(), new string[]{
                "language" + getSettingsFileSeperator() + language.ToString()
                , "theSimsDocumentsFolderPath" + getSettingsFileSeperator() + theSimsDocumentsFolderPath
                , "gameInstallFolderPath" + getSettingsFileSeperator() + gameInstallFolderPath
                , "autoOpenLatest" + getSettingsFileSeperator() + autoOpenLatest
                , "autoOpenLatestBringToFront" + getSettingsFileSeperator() + autoOpenLatestBringToFront
                , "autoReIndex" + getSettingsFileSeperator() + autoReIndex
            });
        }

        public static (bool, List<string>) readSettingsFile()
        {
            List<string> ret = new List<string>();

            string configPath = getSettingsFilePath();
            if (File.Exists(configPath))
            {
                string[] lines = new string[1];
                try
                {
                     lines = File.ReadAllLines(configPath);
                }
                catch (Exception ex)
                {
                    Task<ButtonResult> br = MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        String.Format(lang.Loc.DialogueSettingsFileError, ex.Message),
                        MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync();

                    if(br.Result == ButtonResult.Yes)
                    {
                        Environment.Exit(-10);
                        return (true, ret);
                    }
                }

                foreach (string line in lines)
                {
                    string[] parts = line.Split(getSettingsFileSeperator());
                    if (parts.Length > 1)
                    {
                        switch (parts[0])
                        {
                            case "gameInstallFolderPath":
                                if (!settings.verifyAsTs4InstallFolder(parts[1])) continue;
                                gameInstallFolderPath = parts[1];
                                ret.Add("gameInstallFolderPath");
                                break;

                            case "theSimsDocumentsFolderPath":
                                if (!settings.verifyAsTs4DocumentsFolder(parts[1])) continue;
                                theSimsDocumentsFolderPath = parts[1];
                                ret.Add("theSimsDocumentsFolderPath");
                                break;

                            case "autoOpenLatest":
                                if (parts[1].ToLower().Equals("true"))
                                {
                                    autoOpenLatest = true;
                                    ret.Add("autoOpenLatest");
                                }
                                if (parts[1].ToLower().Equals("false"))
                                {
                                    autoOpenLatest = false;
                                    ret.Add("autoOpenLatest");
                                }
                                break;

                            case "autoOpenLatestBringToFront":
                                if (parts[1].ToLower().Equals("true"))
                                {
                                    autoOpenLatestBringToFront = true;
                                    ret.Add("autoOpenLatestBringToFront");
                                }
                                if (parts[1].ToLower().Equals("false"))
                                {
                                    autoOpenLatestBringToFront = false;
                                    ret.Add("autoOpenLatestBringToFront");
                                }
                                break;

                            case "autoReIndex":
                                if (parts[1].ToLower().Equals("true"))
                                {
                                    autoReIndex = true;
                                    ret.Add("autoReIndex");
                                }
                                if (parts[1].ToLower().Equals("false"))
                                {
                                    autoReIndex = false;
                                    ret.Add("autoReIndex");
                                }
                                break;

                            case "language":
                                foreach(supportedLang l in Enum.GetValues(typeof(supportedLang)))
                                {
                                    if (l.ToString().Equals(parts[1]))
                                    {
                                        language = l;
                                        ret.Add("language");
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                return (false, ret);
            }
            return (true, ret);
        }

        public static void tryFillSettings(List<string> alreadyFilled)
        {

            string eaFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Electronic Arts");
            string enFolder = Path.Combine(eaFolder, "The Sims 4");
            string deFolder = Path.Combine(eaFolder, "Die Sims 4");
            string nlFolder = Path.Combine(eaFolder, "De Sims 4");
            string frFolder = Path.Combine(eaFolder, "Les Sims 4");
            string esFolder = Path.Combine(eaFolder, "Los Sims 4");

            string? potentialTheSimsDocumentsFolderPath = null;
            string? potentialGameInstallFolderPath = null;
            supportedLang? potentialLanguage = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string locale = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Maxis\\The Sims 4", "Locale", "en_US");
                potentialGameInstallFolderPath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Maxis\\The Sims 4", "Install Dir", null);
                potentialTheSimsDocumentsFolderPath = enFolder;
                potentialLanguage = supportedLang.en_US;
                switch (locale)
                {
                    case "zh_CN": potentialLanguage = supportedLang.zh_CN; break;
                    case "zh_TW": potentialLanguage = supportedLang.zh_TW; break;
                    case "cs_CZ": potentialLanguage = supportedLang.cs_CZ; break;
                    case "da_DK": potentialLanguage = supportedLang.da_DK; break;

                    case "nl_NL":
                        potentialTheSimsDocumentsFolderPath = nlFolder;
                        potentialLanguage = supportedLang.nl_NL;
                        break;

                    case "fi_FI": potentialLanguage = supportedLang.fi_FI; break;

                    case "fr_FR":
                        potentialTheSimsDocumentsFolderPath = frFolder;
                        potentialLanguage = supportedLang.fr_FR;
                        break;

                    case "de_DE":
                        potentialTheSimsDocumentsFolderPath = deFolder;
                        potentialLanguage = supportedLang.de_AT;
                        break;

                    case "it_IT": potentialLanguage = supportedLang.it_IT; break;
                    case "ja_JP": potentialLanguage = supportedLang.ja_JP; break;
                    case "ko_KR": potentialLanguage = supportedLang.ko_KR; break;
                    case "nb_NO": potentialLanguage = supportedLang.nb_NO; break;
                    case "pl_PL": potentialLanguage = supportedLang.pl_PL; break;
                    case "ru_RU": potentialLanguage = supportedLang.ru_RU; break;

                    case "es_ES":
                        potentialTheSimsDocumentsFolderPath = esFolder;
                        potentialLanguage = supportedLang.es_ES;
                        break;

                    case "sv_SE": potentialLanguage = supportedLang.sv_SE; break;
                    case "pt_BR": potentialLanguage = supportedLang.pt_BR; break;
                }
            } else {

                theSimsDocumentsFolderPath = enFolder;
                // Just try to determine it through the cultureinfo
                string l = System.Globalization.CultureInfo.CurrentCulture.Name.Split('-')[0];
                string c = System.Globalization.CultureInfo.CurrentCulture.Name.Split('-')[1];
                switch (l)
                {
                    case "de":
                        if (Path.Exists(nlFolder))
                        {
                            potentialTheSimsDocumentsFolderPath = deFolder;
                        }
                        break;
                    case "nl":
                        if (Path.Exists(nlFolder))
                        {
                            potentialTheSimsDocumentsFolderPath = nlFolder;
                        }
                        break;
                    case "es":
                        if (Path.Exists(nlFolder))
                        {
                            potentialTheSimsDocumentsFolderPath = esFolder;
                        }
                        break;
                    case "fr":
                        if (Path.Exists(frFolder))
                        {
                            potentialTheSimsDocumentsFolderPath = frFolder;
                        }
                        break;
                }
            }

            if (!alreadyFilled.Exists(x => x.Equals("theSimsDocumentsFolderPath")) && verifyAsTs4DocumentsFolder(potentialTheSimsDocumentsFolderPath))
            {
                theSimsDocumentsFolderPath = potentialTheSimsDocumentsFolderPath;
            }

            if (!alreadyFilled.Exists(x => x.Equals("gameInstallFolderPath")) && verifyAsTs4InstallFolder(potentialGameInstallFolderPath))
            {
                gameInstallFolderPath = potentialGameInstallFolderPath;
            }
        }

        public static bool verifyAsTs4DocumentsFolder(string path, bool showMessages=false, string appendToMessage="")
        {
            if (path == null) return false;

            if (!Directory.Exists(path))
            {
                if (showMessages)
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        lang.Loc.DialogueVerifyPathDoesNotExist,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
                return false;
            }

            if(!Directory.Exists(Path.Join(path, "Mods")) || !File.Exists(Path.Join(path, "GameVersion.txt")))
            {
                if (showMessages)
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        lang.Loc.DialogueVerifyTs4DocumentsFolderSubfolderError,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
                return false;
            }

            return true;
        }

        public static bool verifyAsTs4InstallFolder(string? path, bool showMessages = false, string appendToMessage = "")
        {
            if (path == null) return false;

            if (!Directory.Exists(path))
            {
                if (showMessages)
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        lang.Loc.DialogueVerifyPathDoesNotExist,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
                return false;
            }

            if( !Directory.Exists(Path.Join(path, "Data")) || !Directory.Exists(Path.Join(path, "Game")))
            {
                if (showMessages)
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        lang.Loc.DialogueVerifyInstallFolderSubfolderError,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
                return false;
            }

            string pythonZipFolder = Path.Join(path, "Data", "Simulation", "Gameplay");
            if ( !Directory.Exists(pythonZipFolder) 
                || !File.Exists(Path.Join(pythonZipFolder, "base.zip"))
                || !File.Exists(Path.Join(pythonZipFolder, "core.zip"))
                || !File.Exists(Path.Join(pythonZipFolder, "simulation.zip")))
            {
                if (showMessages)
                {
                    MessageBoxManager.GetMessageBoxStandard(
                        lang.Loc.DialogueGeneralError,
                        lang.Loc.DialogueVerifyInstallFolderNoPythonZips,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                }
                return false;
            }

            return true;
        }
    }
}
