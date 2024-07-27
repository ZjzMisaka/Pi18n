using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pi18n
{
    public class ResourceManager : INotifyPropertyChanged
    {
        private static readonly Lazy<ResourceManager> s_instance = new Lazy<ResourceManager>(() => new ResourceManager());
        private Dictionary<string, string> _currentResourceDict;
        private CultureInfo _currentCultureInfo;
        private Dictionary<string, List<string>> _languageDict;
        private List<CultureInfo> _cultureList;

        /// <summary>
        /// Get instance of ResourceManager
        /// </summary>
        public static ResourceManager Instance => s_instance.Value;
        /// <summary>
        /// Get current CultureInfo instance
        /// </summary>
        public static CultureInfo CurrentCulture => Instance._currentCultureInfo;
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
        /// Sets up the ResourceManager with the appropriate resource path and naming convention.
        /// </summary>
        /// <param name="path">resource path</param>
        /// <param name="format">naming convention</param>
        public static void SetUp(string path, string format)
        {
            Instance.SetUpInstance(path, format);
        }

        private void SetUpInstance(string path, string format)
        {
            _cultureList = new List<CultureInfo>();
            _languageDict = new Dictionary<string, List<string>>();

            string patternFile = format.Replace("{I18N}", @"([a-z]{2}-[A-Z]{2})").Replace("{ANY}", @"(.*)");
            List<int> indexList = new List<int>();
            int cultureIndex = format.IndexOf("{I18N}");
            int anyIndex = format.IndexOf("{ANY}");
            while (anyIndex != -1)
            {
                indexList.Add(anyIndex);
                anyIndex = format.IndexOf("{ANY}", anyIndex + 1);
            }
            indexList.Add(cultureIndex);
            indexList.Sort();
            int matchGroupIndex = indexList.IndexOf(cultureIndex) + 1;

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                Match match = Regex.Match(fileName, patternFile);

                if (match.Success)
                {
                    if (match.Groups.Count > 1)
                    {
                        if (!_languageDict.ContainsKey(match.Groups[matchGroupIndex].Value))
                        {
                            _languageDict[match.Groups[matchGroupIndex].Value] = new List<string>();
                            _cultureList.Add(new CultureInfo(match.Groups[matchGroupIndex].Value, false));
                        }
                        _languageDict[match.Groups[matchGroupIndex].Value].Add(file);
                    }
                }
            }
        }

        /// <summary>
        /// Sets or switches the current language by culture code.
        /// </summary>
        /// <param name="cultureCode">culture code</param>
        public static void SetLanguage(string cultureCode)
        {
            Instance.SetLanguageInstance(cultureCode);
        }

        private void SetLanguageInstance(string cultureCode)
        {
            CultureInfo newCulture = CultureInfoList.Where((x) => x.Name == cultureCode).FirstOrDefault();
            if (newCulture == null)
            {
                return;
            }

            CultureInfo oldCulture = CurrentCulture;
            _currentCultureInfo = newCulture;
            _currentResourceDict = new Dictionary<string, string>();

            foreach (string path in _languageDict[cultureCode])
            {
                LoadResourceFile(path);
            }

            OnPropertyChanged(null);
            LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldCulture, CurrentCulture));
        }

        private void LoadResourceFile(string filePath)
        {
            foreach (string line in File.ReadAllLines(filePath))
            {
                string[] parts = Regex.Split(line, @"(?<!\\)=");
                if (parts.Length == 2)
                {
                    parts[0].Replace("\\=", "=");
                    parts[1].Replace("\\=", "=");
                    _currentResourceDict[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
