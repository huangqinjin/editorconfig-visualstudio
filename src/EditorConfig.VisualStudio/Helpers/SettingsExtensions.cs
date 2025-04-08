using EditorConfig.Core;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EditorConfig.VisualStudio.Helpers
{
    internal static class ConfigLoader
    {
        public static bool TryLoad(string path, out FileConfiguration fileConfiguration)
        {
            fileConfiguration = null;
            if (string.IsNullOrEmpty(path)) return false;
            var parser = new EditorConfigParser();
            var configFiles = parser.GetConfigurationFilesTillRoot(path);
            if (configFiles.Count == 0 || !configFiles.First().IsRoot)
            {
                var root = new DirectoryInfo(path).Root.FullName;
                var configFile = Path.Combine(root, parser.ConfigFileName);
                if (File.Exists(configFile))
                {
                    configFiles.Insert(0, EditorConfigFile.Parse(configFile));
                }
            }
            fileConfiguration = parser.Parse(path, configFiles);
            if ((fileConfiguration?.Properties.Count ?? 0) == 0)
                fileConfiguration = null;
            return fileConfiguration != null;
        }
    }

    internal static class SettingsExtensions
    {
        private static readonly Regex Cr = new Regex("^cr", RegexOptions.Compiled);
        private static readonly Regex Lf = new Regex("lf$", RegexOptions.Compiled);

        internal static bool IfHasKeyTrySetting(this FileConfiguration results, string key, Action<int> setter)
        {
            if (!results.Properties.ContainsKey(key)) return false;
            int value;
            if (!int.TryParse(results.Properties[key], NumberStyles.Integer, null, out value)) return false;
            setter(value);
            return true;
        }

        internal static bool TryKeyAsBool(this FileConfiguration results, string key)
        {
            return results.Properties.ContainsKey(key) && results.Properties[key] == "true";
        }

        internal static string EndOfLine(this FileConfiguration results)
        {
            const string key = "end_of_line";
            return results.Properties.ContainsKey(key) ? Lf.Replace(Cr.Replace(results.Properties[key], "\r"), "\n") : null;
        }
    }
}
