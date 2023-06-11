using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ModAssistant.Properties;
using static ModAssistant.Http;

namespace ModAssistant
{
    public static class ZeyuCount
    {
        private static int limit = 200;
        private static int count = 0;
        private static string date = $"{DateTime.Now.Date.Year}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Day}";

        private const int beatsaver_single_multiplier = 1;
        private const int beatsaver_playlist_multiplier = 3;
        private const int modelsaber_multiplier = 1;

        public class ZeyuCountData
        {
            public string date { get; set; }
            public int count { get; set; }
        }

        public static void loadData()
        {
            string prev_data = Settings.Default.ZeyuCount;
            if (!string.IsNullOrEmpty(prev_data))
            {
                ZeyuCountData data = JsonSerializer.Deserialize<ZeyuCountData>(Settings.Default.ZeyuCount);
                date = data.date;
                count = data.count;
            }
            else {
                saveData();
            }
        }

        public static void saveData()
        {
            ZeyuCountData data = new ZeyuCountData();
            data.date = date;
            data.count = count;
            Settings.Default.ZeyuCount = JsonSerializer.Serialize(data);
            Settings.Default.Save();
        }

        private static bool CheckCount(int chance)
        {
            updateCount(0);
            return (count + chance <= limit);
        }

        private static bool updateCount(int chance)
        {
            loadData();
            if (string.IsNullOrEmpty(date))
            {
                date = $"{DateTime.Now.Date.Year}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Day}";
            }

            if (date != $"{DateTime.Now.Date.Year}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Day}")
            {
                date = $"{DateTime.Now.Date.Year}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Day}";
                count = 0;
            }

            if (count + chance > limit)
            {
                return false;
            }
            else
            {
                count += chance;
                saveData();
                Utils.UpdateCountIndicator();
                return true;
            }
        }

        public static int getCount() {
            return limit - count;
        }

        public static bool checkBeatsaverSingle()
        {
            return CheckCount(beatsaver_single_multiplier);
        }

        public static bool checkBeatsaverMultiple()
        {
            return CheckCount(beatsaver_playlist_multiplier);
        }

        public static bool checkModelSaberSingle()
        {
            return CheckCount(modelsaber_multiplier);
        }

        public static void downloadBeatsaverSingle()
        {
            if (updateCount(beatsaver_single_multiplier))
            {
                // When Success
            }
            else
            {
                // When False
            }
        }

        public static void downloadBeatsaverMultiple()
        {
            if (updateCount(beatsaver_playlist_multiplier))
            {
                // When Success
            }
            else
            {
                // When False
            }
        }

        public static void downloadModelSaberSingle()
        {
            if (updateCount(modelsaber_multiplier))
            {
                // When Success
            }
            else
            {
                // When False
            }
        }
    }
}
