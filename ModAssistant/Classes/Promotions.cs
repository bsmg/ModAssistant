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
