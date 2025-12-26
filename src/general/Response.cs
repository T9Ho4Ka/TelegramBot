namespace TelegramBot.general;

public class Response
{
    public static async Task LogResponseTime(ITelegramBotClient bot, Message msg, Stopwatch stopwatch) {
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        var logMessage = $"[LOG] Program execution time is {elapsedMs} ms.)";
        Console.WriteLine(logMessage);
    }
}