namespace TinBot.Commands
{
    public enum EActionType
    {
        Speak,
        Face
    }

    public class TinBotAction
    {
        public string Name { get; set; }
        public EActionType Type { get; set; }

        public string SpeakText { get; set; }
        public string SpeakKey { get; set; }

        private TinBotAction()
        {
            
        }

        public static TinBotAction MakeSpeakAction(string name, string key, string customText = null)
        {
            var tba = new TinBotAction()
            {
                Type = EActionType.Speak,
                Name =  name,
                SpeakKey = key,
                SpeakText = customText
            };

            return tba;
        }
    }
}
