using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModAssistant.Pages;

namespace ModAssistant
{
    class Languages
    {
        public static string LoadedLanguage { get; private set; }
        public static List<CultureInfo> LoadedLanguages { get => availableCultures.ToList(); }

        private static string[] availableLanguageCodes = { "de", "en", "fr", "it", "ko", "nl", "ru", "zh" };

        private static IEnumerable<CultureInfo> availableCultures;
        public static void LoadLanguages()
        {
            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            // Get CultureInfo for any of the available translations
            availableCultures = allCultures.Where(cultureInfo => availableLanguageCodes.Any(code => code.Equals(cultureInfo.Name)));

            string savedLanguageCode = Properties.Settings.Default.LanguageCode;

            if (savedLanguageCode.Length == 0)
            {
                // If no language code was saved, load system language
                savedLanguageCode = CultureInfo.CurrentUICulture.Name.Split('-').First();
            }
            else if (!availableLanguageCodes.Any(code => code.Equals(savedLanguageCode)))
            {
                // If language code isn't supported, load English instead
                savedLanguageCode = "en";
            }

            if (Options.Instance != null && Options.Instance.LanguageSelectComboBox != null)
            {
                Options.Instance.LanguageSelectComboBox.ItemsSource = availableCultures.Select(cultureInfo => cultureInfo.NativeName).ToList();
                Options.Instance.LanguageSelectComboBox.SelectedIndex = LoadedLanguages.FindIndex(cultureInfo => cultureInfo.Name.Equals(savedLanguageCode)); ;
            }

            LoadLanguage(savedLanguageCode);
        }

        private static ResourceDictionary LanguagesDict
        {
            get
            {
                return Application.Current.Resources.MergedDictionaries[1];
            }
        }

        public static void LoadLanguage(string languageCode)
        {
            try
            {
                LanguagesDict.Source = new Uri($"Localisation/{languageCode}.xaml", UriKind.Relative);
                Properties.Settings.Default.LanguageCode = languageCode;
                Properties.Settings.Default.Save();
            }
            catch (IOException)
            {
                if (languageCode.Contains("-"))
                {
                    LoadLanguage(languageCode.Split('-').First());
                }
                // Can't load language file
            }
        }
    }
}
