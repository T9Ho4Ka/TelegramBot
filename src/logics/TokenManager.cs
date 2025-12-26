namespace TelegramBot.logics;

public class TokenManager {
    private static string RequestTokenFromUser() {
        Console.Write("Insert your token to continue: ");
        string? token = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(token)) {
            Console.WriteLine("Token cannot be empty. Please try again.");
            return RequestTokenFromUser();
        }
        Console.WriteLine($"\nYour token: {token}\nContinue? Yes(y)/No(n)");
        char key = Console.ReadKey(intercept: true).KeyChar;
        if (char.ToLower(key) == 'y') {
            Environment.SetEnvironmentVariable(Constants.EnvironmentalVariable, token, EnvironmentVariableTarget.User);
            Console.WriteLine("\nToken has been saved.");
            return token;
        }
        Console.WriteLine("\nCanceled.");
        Environment.Exit(-1);
        return string.Empty;
    }

    private static string GetOrRequestToken() {
        string? token = Environment.GetEnvironmentVariable(Constants.EnvironmentalVariable, EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(token)) {
            Console.WriteLine("[ERROR] Environment variable (bot_token) not found.");
            return RequestTokenFromUser();
        }
        return token;
    }

    private static bool TryInitBot(string token, out TelegramBotClient? bot) {
        try {
            bot = new TelegramBotClient(token);
            bot.GetMe().Wait(); // token check (sync)
            Console.WriteLine("Token is valid. Bot initialized successfully!");
            return true;
        }
        catch (Exception ex) {
            Console.WriteLine($"[ERROR] Bot initialization failed: {ex.Message}");
            bot = null;
            return false;
        }
    }

    public static TelegramBotClient InitTokenAsync() {
        string botToken = GetOrRequestToken();
        if (!TryInitBot(botToken, out var bot)) {
            Console.WriteLine("Try entering your token again.\n");
            botToken = RequestTokenFromUser();

            if (!TryInitBot(botToken, out bot)) {
                Console.WriteLine("[ERROR] Failed to initialize bot after retry. Exiting.");
                Environment.Exit(-1);
            }
        }
        return bot;
    }

}
