using System.Collections.Generic;

namespace ModAssistant
{
    class Promotions
    {
        public static Promotion[] ActivePromotions =
        {
            new Promotion
            {
                ModName = "YUR Fit Calorie Tracker",
                Links = new List<PromotionLink>(){
                    new PromotionLink{Text = "Join our Discord!", Link = "https://yur.chat", TextAfterLink = " Or find us on "},
                    new PromotionLink{Text = "iOS", Link = "https://testflight.apple.com/join/GsTrCPFE", TextAfterLink = " and " },
                    new PromotionLink{Text = "Android", Link = "https://play.google.com/store/apps/details?id=com.yur", TextAfterLink = "!" }
                },
            }
        };
    }

    class Promotion
    {
        public string ModName;
        public List<PromotionLink> Links;
    }

    class PromotionLink
    {
        public string Text;
        public string Link;
        public string TextAfterLink;
    }
}
