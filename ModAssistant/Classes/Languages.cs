using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using ModAssistant.Pages;

namespace ModAssistant
{
    internal class Languages
    {
        public static string LoadedLanguage { get; private set; }
        public static List<CultureInfo> LoadedLanguages => availableCultures.ToList();
        public static bool FirstRun = true;
        private static readonly string[] availableLanguageCodes = { "de", "en", "es", "fr", "it", "ja", "ko", "nb", "nl", "pl", "ru", "sv", "th", "zh" };
        private static IEnumerable<CultureInfo> availableCultures;

        public static void LoadLanguages()
        {
            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            // Get CultureInfo for any of the available translations
            availableCultures = allCultures.Where(cultureInfo => availableLanguageCodes.Any(code => code.Equals(cultureInfo.Name)));

            string savedLanguageCode = Properties.Settings.Default.LanguageCode;
            if (!LoadLanguage(savedLanguageCode))
            {
                // If no language code was saved, load system language
                if (!LoadLanguage(CultureInfo.CurrentUICulture.Name))
                {
                    _ = LoadLanguage("en");
                }
            }

            UpdateUI(LoadedLanguage);
        }

        public static void UpdateUI(string languageCode)
        {
            if (Options.Instance != null && Options.Instance.LanguageSelectComboBox != null)
            {
                Options.Instance.LanguageSelectComboBox.ItemsSource = availableCultures.Select(cultureInfo => cultureInfo.NativeName).ToList();
                Options.Instance.LanguageSelectComboBox.SelectedIndex = LoadedLanguages.FindIndex(cultureInfo => cultureInfo.Name.Equals(languageCode));
            }
        }

        public static ResourceDictionary LanguagesDict
        {
            get
            {
                return Application.Current.Resources.MergedDictionaries[1];
            }
        }

        public static bool LoadLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return false;
            }

            try
            {
                LanguagesDict.Source = new Uri($"Localisation/{languageCode}.xaml", UriKind.Relative);
                LoadedLanguage = languageCode;
                return true;
            }
            catch (IOException)
            {
                if (languageCode.Contains("-"))
                {
                    return LoadLanguage(languageCode.Split('-').First());
                }

                return false;
            }
        }
    }
}
