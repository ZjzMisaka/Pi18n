using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pi18n
{
    public enum CultureType { CurrentUICulture, CurrentCulture }
    public class ResourceManager : DynamicObject, INotifyPropertyChanged
    {
        private static readonly Lazy<ResourceManager> s_instance = new Lazy<ResourceManager>(() => new ResourceManager());
        private CultureInfo _defaultCulture;
        private CultureInfo _currentCulture;
        private Dictionary<string, List<string>> _languageDict;
        private List<CultureInfo> _cultureList;
        private ExpandoObject _dynamicProperties;

        /// <summary>
        /// Get instance of ResourceManager
        /// </summary>
        public static dynamic Instance => s_instance.Value;

        private static ResourceManager ResourceManagerInstance => s_instance.Value;

        /// <summary>
        /// Get or set default CultureInfo instance
        /// </summary>
        public static CultureInfo DefaultCulture
        {
            get => ResourceManagerInstance._defaultCulture;
            set => SetDefault(value);
        }

        /// <summary>
        /// Get or set current CultureInfo instance
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => ResourceManagerInstance._currentCulture;
            set => SetLanguage(value);
        }
        /// <summary>
        /// Get list of CultureInfo instance
        /// </summary>
        public static List<CultureInfo> CultureInfoList => ResourceManagerInstance._cultureList;
        /// <summary>
        /// Get list of culture code (like "en-US")
        /// </summary>
        public static List<string> CultureCodeList => ResourceManagerInstance._cultureList.Select((x) => x.Name).ToList();
        /// <summary>
        /// Get list of culture name (like "English (United States)")
        /// </summary>
        public static List<string> CultureNameList => ResourceManagerInstance._cultureList.Select((x) => x.NativeName).ToList();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Language changed
        /// </summary>
        public static event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        /// <summary>
        /// If true, return the key if not found in the resource file.
        /// </summary>
        public static bool ReturnKeyIfNotFound { get; set; } = false;

        /// <summary>
        /// Default content if the key is not found in the resource file.
        /// </summary>
        public static string DefaultContent { set; get; } = "NOT FOUND";

        private ResourceManager()
        {
            _dynamicProperties = new ExpandoObject();

            _cultureList = new List<CultureInfo>();
            _languageDict = new Dictionary<string, List<string>>();
        }

        public string this[string key]
        {
            get
            {
                if (key == null)
                {
                    return ReturnKeyIfNotFound ? key : DefaultContent;
                }
                var dict = (IDictionary<string, object>)_dynamicProperties;
                return dict.ContainsKey(key) ? (string)dict[key] : ReturnKeyIfNotFound ? key : DefaultContent;
            }
        }

        /// <summary>
        /// Get formatted string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns>Formatted string</returns>
        public static string GetFormat(string key, params object[] args)
        {
            return string.Format(ResourceManagerInstance[key], args);
        }

        /// <summary>
        /// Sets up the ResourceManager with the appropriate resource path and naming convention.
        /// </summary>
        /// <param name="path">resource path</param>
        /// <param name="fileFormat">naming convention</param>
        public static void SetUp(string path, string fileFormat)
        {
            ResourceManagerInstance.SetUpInstance(path, fileFormat);
        }

        /// <summary>
        /// Resets the ResourceManager to its initial state, clearing all loaded resources and settings.
        /// </summary>
        public static void Reset()
        {
            var instance = ResourceManagerInstance;
            instance._cultureList = new List<CultureInfo>();
            instance._languageDict = new Dictionary<string, List<string>>();
            instance._dynamicProperties = new ExpandoObject();
            instance._defaultCulture = null;
            instance._currentCulture = null;
        }

        private void SetUpInstance(string path, string format)
        {
            string filePatternRegex = Regex.Escape(format)
                .Replace(Regex.Escape("{I18N}"), @"([a-zA-Z\-]+)")
                .Replace(Regex.Escape("{ANY}"), @"(.*)");
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

                if (match.Success && match.Groups.Count > 1)
                {
                    string cultureName = match.Groups[cultureGroupIndex].Value;

                    if (!_languageDict.ContainsKey(cultureName))
                    {
                        _languageDict[cultureName] = new List<string>();
                        _cultureList.Add(new CultureInfo(cultureName, false));
                    }

                    _languageDict[cultureName].Add(file);
                }
            }

            if (CurrentCulture != null)
            {
                SetLanguage(CurrentCulture);
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

            SetDefaultInstance(cultureInfo);

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

            SetDefaultInstance(cultureInfo);

            return true;
        }

        /// <summary>
        /// Sets the default language by culture type (CurrentUICulture | CurrentCulture).
        /// </summary>
        /// <param name="cultureType">Culture type</param>
        public static bool SetDefault(CultureType cultureType)
        {
            CultureInfo cultureInfo = CultureInfoList.Where((x) => x.Name == (cultureType == CultureType.CurrentUICulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentCulture.Name)).FirstOrDefault();
            if (cultureInfo == null)
            {
                return false;
            }

            SetDefaultInstance(cultureInfo);

            return true;
        }

        private static void SetDefaultInstance(CultureInfo cultureInfo)
        {
            ResourceManagerInstance._defaultCulture = cultureInfo;

            if (CurrentCulture == null)
            {
                SetLanguage(cultureInfo);
            }
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

            ResourceManagerInstance.SetLanguageInstance(cultureInfo);

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

            ResourceManagerInstance.SetLanguageInstance(cultureInfo);

            return true;
        }

        private static CultureInfo GetCultureInfo(string cultureCode)
        {
            CultureInfo cultureInfo = CultureInfoList.Where((x) => x.Name == cultureCode).FirstOrDefault();
            if (cultureInfo == null)
            {
                cultureInfo = ResourceManagerInstance._defaultCulture;
            }
            return cultureInfo;
        }

        private void SetLanguageInstance(CultureInfo newCulture)
        {
            CultureInfo oldCulture = CurrentCulture;
            _currentCulture = newCulture;
            _dynamicProperties = new ExpandoObject();

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
                lines = File.ReadAllLines(filePath, Encoding.UTF8);
            }
            catch
            {
                return;
            }

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                if (TryParseLine(line, out string key, out string value))
                {
                    ((IDictionary<string, object>)_dynamicProperties)[key] = value;
                }
            }
        }

        private static bool TryParseLine(string line, out string key, out string value)
        {
            var sb = new StringBuilder();
            int i = 0;
            int n = line.Length;
            bool keyDone = false;
            key = value = null;
            var keySb = new StringBuilder();
            var valSb = new StringBuilder();

            while (i < n)
            {
                char c = line[i];
                if (c == '\\' && i + 1 < n)
                {
                    char next = line[i + 1];
                    char decoded;
                    switch (next)
                    {
                        case '\\':
                            decoded = '\\';
                            break;
                        case '=':
                            decoded = '=';
                            break;
                        case 'n':
                            decoded = '\n';
                            break;
                        default:
                            decoded = next;
                            break;
                    }
                    (keyDone ? valSb : keySb).Append(decoded);
                    i += 2;
                    continue;
                }
                if (c == '=' && !keyDone)
                {
                    keyDone = true;
                    i++;
                    continue;
                }
                (keyDone ? valSb : keySb).Append(c);
                i++;
            }

            if (!keyDone)
            {
                return false;
            }

            key = keySb.ToString();
            value = valSb.ToString();
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var propertiesDict = (IDictionary<string, object>)_dynamicProperties;
            propertiesDict[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertiesDict = (IDictionary<string, object>)_dynamicProperties;
            return propertiesDict.TryGetValue(binder.Name, out result);
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
