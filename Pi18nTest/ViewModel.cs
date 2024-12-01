using System.Dynamic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pi18n;

namespace Pi18nTest
{
    public class ViewModel : ObservableObject
    {
        public dynamic Resource => ResourceManager.Instance;

        private CultureInfo _selectedLanguage;
        public CultureInfo SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                SetProperty<CultureInfo>(ref _selectedLanguage, value);
                ResourceManager.CurrentCulture = value;
                OnPropertyChanged(nameof(NowLanguageName));
            }
        }

        private string _input;
        public string Input
        {
            get => _input;
            set
            {
                SetProperty<string>(ref _input, value);
                FormattedText = ResourceManager.GetFormat("Format", value);
            }
        }

        private string _formattedText;
        public string FormattedText
        {
            get => _formattedText;
            set
            {
                SetProperty<string>(ref _formattedText, value);
            }
        }

        private string _cultureCode;
        public string CultureCode
        {
            get => _cultureCode;
            set
            {
                SetProperty<string>(ref _cultureCode, value);
                ResourceManager.SetLanguage(value);
                OnPropertyChanged(nameof(NowLanguageName));
                SelectedLanguage = ResourceManager.CurrentCulture;
                try
                {
                    CultureText = CultureInfo.GetCultureInfo(value, false).NativeName;
                }
                catch
                {
                    CultureText = "";
                }
            }
        }
        private string _cultureText;
        public string CultureText
        {
            get => _cultureText;
            set
            {
                SetProperty<string>(ref _cultureText, value);
            }
        }

        public string NowLanguageName => ResourceManager.CurrentCulture.NativeName;

        public ICommand MessageBoxCommand { get; set; }

        public ViewModel()
        {
            ResourceManager.SetUp("resource", "language-resource-{I18N}.{ANY}.i18n");
            if (!ResourceManager.SetDefault(CultureType.CurrentCulture))
            {
                ResourceManager.SetDefault(ResourceManager.CultureInfoList[0]);
            }
            SelectedLanguage = ResourceManager.CurrentCulture;
            MessageBoxCommand = new RelayCommand(ShowMessageBox);

            ResourceManager.LanguageChanged += (s, e) =>
            {
                FormattedText = ResourceManager.GetFormat("Format", Input);
            };
        }

        private void ShowMessageBox()
        {
            MessageBox.Show(Resource["MessageText"]);
            MessageBox.Show(Resource.MessageText);
        }
    }
}
