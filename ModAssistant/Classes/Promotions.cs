using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModAssistant
{
    class Promotions
    {
        public static Promotion[] ActivePromotions =
        {
            new Promotion
            {
                ModName = "YUR Fit Calorie Tracker",
                Text = "Join our Discord!",
                Link = "https://yur.chat"
            }
        };
    }

    class Promotion
    {
        public string ModName;
        public string Text;
        public string Link;
    }
}
