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
            availableCultures = allCultures.Where(cultureInfo => availableLanguageCodes.Any(code => code.CompareTo(cultureInfo.Name) == 0));

            if (Options.Instance != null && Options.Instance.LanguageSelectComboBox != null)
            {
                Options.Instance.LanguageSelectComboBox.ItemsSource = availableCultures.Select(cultureInfo => cultureInfo.NativeName).ToList();
            }
        }

        private static ResourceDictionary LanguagesDict
        {
            get
            {
                return Application.Current.Resources.MergedDictionaries[1];
            }
        }

        public static void LoadLanguage(string culture)
        {
            try
            {
                LanguagesDict.Source = new Uri($"Localisation/{culture}.xaml", UriKind.Relative);
            }
            catch (IOException)
            {
                if (culture.Contains("-"))
                {
                    LoadLanguage(culture.Split('-').First());
                }
                // Can't load language file
            }
        }
    }
}
