namespace TelegramBot.general;

public class Response
{
    public static async Task LogResponseTime(Stopwatch stopwatch, ITelegramBotClient bot, Message msg) {
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        var logMessage = $"[LOG] Program execution time is {elapsedMs} ms.)";
        
        Console.WriteLine(logMessage);
        await bot.SendMessage(
            chatId: msg.Chat.Id,
            text: logMessage);
    }
}