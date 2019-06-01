namespace ModAssistant.Classes
{
    public partial class Mod
    {
        public class Dependency
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public Classes.Mod Mod { get; set; }
        }
    }
}