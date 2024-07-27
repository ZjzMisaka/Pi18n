using System.Globalization;

namespace Pi18n
{
    public class LanguageChangedEventArgs : EventArgs
    {
        public CultureInfo OldCultureInfo { get; }
        public CultureInfo NewCultureInfo { get; }

        public LanguageChangedEventArgs(CultureInfo oldCultureInfo, CultureInfo newCultureInfo)
        {
            OldCultureInfo = oldCultureInfo;
            NewCultureInfo = newCultureInfo;
        }
    }
}
