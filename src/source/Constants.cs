namespace TelegramBot.source;
    public class Constants {
        public const string Prefix = ".";
        public const char OptionPrefix = '-';
        public const string EnvironmentalVariable = "TELEGRAM_BOT_TOKEN";

        public const string AvatarsPath = @"../../../data/cache/avatars";
        public const string TemplatesPath = @"../../../data/templates";
        public const string DefaultTemplatePath = @"../../../data/templates/default.jpg";
        public const string DefaultAvatarPath = @"../../../data/cache/avatars/default.jpg";

        public const string VERSION = "ALPHA 0.0.1";
        public const bool IsCommandsConsidered = false;
        public const float RequiredExpForFirstLvl = 100f;

        
        public const string CommandList = $"""
                                        <b><u>Command menu</u></b>:
                                        """;
        
    }
