using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pi18n;

namespace Pi18nTest
{
    public class ViewModel : ObservableObject
    {
        public ResourceManager ResourceManager => ResourceManager.Instance;

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

        public string NowLanguageName => ResourceManager.CurrentCulture.NativeName;

        public ICommand MessageBoxCommand { get; set; }

        public ViewModel()
        {
            ResourceManager.SetUp("resource", "language-resource-{I18N}.{ANY}.i18n");
            SelectedLanguage = ResourceManager.CultureInfoList[0];
            MessageBoxCommand = new RelayCommand(ShowMessageBox);
        }

        private void ShowMessageBox()
        {
            MessageBox.Show(ResourceManager["MessageText"]);
        }
    }
}
