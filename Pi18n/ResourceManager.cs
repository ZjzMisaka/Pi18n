using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pi18n
{
    public class ResourceManager : INotifyPropertyChanged
    {
        private static readonly Lazy<ResourceManager> s_instance = new Lazy<ResourceManager>(() => new ResourceManager());
        private Dictionary<string, string> _currentResourceDict;
        private CultureInfo _defaultCulture;
        private CultureInfo _currentCulture;
        private Dictionary<string, List<string>> _languageDict;
        private List<CultureInfo> _cultureList;

        /// <summary>
        /// Get instance of ResourceManager
        /// </summary>
        public static ResourceManager Instance => s_instance.Value;

        /// <summary>
        /// Get or set default CultureInfo instance
        /// </summary>
        public static CultureInfo DefaultCulture
        {
            get => Instance._defaultCulture;
            set => SetDefault(value);
        }

        /// <summary>
        /// Get or set current CultureInfo instance
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => Instance._currentCulture;
            set => SetLanguage(value);
        }
        /// <summary>
        /// Get list of CultureInfo instance
        /// </summary>
        public static List<CultureInfo> CultureInfoList => Instance._cultureList;
        /// <summary>
        /// Get list of culture code (like "en-US")
        /// </summary>
        public static List<string> CultureCodeList => Instance._cultureList.Select((x) => x.Name).ToList();
        /// <summary>
        /// Get list of culture name (like "English (United States)")
        /// </summary>
        public static List<string> CultureNameList => Instance._cultureList.Select((x) => x.NativeName).ToList();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Language changed
        /// </summary>
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        private ResourceManager()
        {
            _currentResourceDict = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get => _currentResourceDict.ContainsKey(key) ? _currentResourceDict[key] : "NOT FOUND";
        }

        /// <summary>
        /// Get formatted string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns>Formatted string</returns>
        public string GetFormat(string key, params object[] args)
        {
            return string.Format(this[key], args);
        }

        /// <summary>
        /// Sets up the ResourceManager with the appropriate resource path and naming convention.
        /// </summary>
        /// <param name="path">resource path</param>
        /// <param name="fileFormat">naming convention</param>
        public static void SetUp(string path, string fileFormat)
        {
            Instance.SetUpInstance(path, fileFormat);
        }

        private void SetUpInstance(string path, string format)
        {
            _cultureList = new List<CultureInfo>();
            _languageDict = new Dictionary<string, List<string>>();

            string filePatternRegex = format.Replace("{I18N}", @"([a-z]{2}-[A-Z]{2})").Replace("{ANY}", @"(.*)");
            List<int> placeholderIndexes = new List<int>();
            int cultureIndex = format.IndexOf("{I18N}");
            int anyIndex = format.IndexOf("{ANY}");
            while (anyIndex != -1)
            {
                placeholderIndexes.Add(anyIndex);
                anyIndex = format.IndexOf("{ANY}", anyIndex + 1);
            }
            placeholderIndexes.Add(cultureIndex);
            placeholderIndexes.Sort();
            int cultureGroupIndex = placeholderIndexes.IndexOf(cultureIndex) + 1;

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                Match match = Regex.Match(fileName, filePatternRegex);

                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (!_languageDict.ContainsKey(match.Groups[cultureGroupIndex].Value))
                        {
                            _languageDict[match.Groups[cultureGroupIndex].Value] = new List<string>();
                            _cultureList.Add(new CultureInfo(match.Groups[cultureGroupIndex].Value, false));
                        }
                        _languageDict[match.Groups[cultureGroupIndex].Value].Add(file);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the default language by CultureInfo object.
        /// </summary>
        /// <param name="cultureInfo">CultureInfo object</param>
        public static bool SetDefault(CultureInfo cultureInfo)
        {
            cultureInfo = CultureInfoList.Where((x) => x.Name == cultureInfo.Name).FirstOrDefault();
            if (cultureInfo == null)
            {
                return false;
            }

            Instance._defaultCulture = cultureInfo;

            return true;
        }

        /// <summary>
        /// Sets the default language by culture code.
        /// </summary>
        /// <param name="cultureCode">culture code</param>
        public static bool SetDefault(string cultureCode)
        {
            CultureInfo cultureInfo = CultureInfoList.Where((x) => x.Name == cultureCode).FirstOrDefault();
            if (cultureInfo == null)
            {
                return false;
            }

            Instance._defaultCulture = cultureInfo;

            return true;
        }

        /// <summary>
        /// Sets or switches the current language by CultureInfo object.
        /// </summary>
        /// <param name="cultureInfo">CultureInfo object</param>
        public static bool SetLanguage(CultureInfo cultureInfo)
        {
            if (cultureInfo != null && !CultureInfoList.Contains(cultureInfo))
            {
                cultureInfo = GetCultureInfo(cultureInfo.Name);
            }
            if (cultureInfo == null)
            {
                return false;
            }

            Instance.SetLanguageInstance(cultureInfo);

            return true;
        }

        /// <summary>
        /// Sets or switches the current language by culture code.
        /// </summary>
        /// <param name="cultureCode">culture code</param>
        public static bool SetLanguage(string cultureCode)
        {
            CultureInfo cultureInfo = GetCultureInfo(cultureCode);
            if (cultureInfo == null)
            {
                return false;
            }

            Instance.SetLanguageInstance(cultureInfo);

            return true;
        }

        private static CultureInfo GetCultureInfo(string cultureCode)
        {
            CultureInfo cultureInfo = CultureInfoList.Where((x) => x.Name == cultureCode).FirstOrDefault();
            if (cultureInfo == null)
            {
                cultureInfo = Instance._defaultCulture;
            }
            return cultureInfo;
        }

        private void SetLanguageInstance(CultureInfo newCulture)
        {
            CultureInfo oldCulture = CurrentCulture;
            _currentCulture = newCulture;
            _currentResourceDict = new Dictionary<string, string>();

            foreach (string path in _languageDict[newCulture.Name])
            {
                LoadResourceFile(path);
            }

            OnPropertyChanged(null);
            LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldCulture, CurrentCulture));
        }

        private void LoadResourceFile(string filePath)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch
            {
                return;
            }
            foreach (string line in lines)
            {
                string[] parts = Regex.Split(line, @"(?<!\\)=");
                if (parts.Length == 2)
                {
                    parts[0] = parts[0].Replace("\\=", "=");
                    parts[1] = parts[1].Replace("\\=", "=");
                    _currentResourceDict[parts[0]] = parts[1].Replace("\\n", "\n");
                }
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
