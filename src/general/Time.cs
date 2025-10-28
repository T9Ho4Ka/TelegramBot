
namespace TelegramBot.general;

public class Time {
    public static async Task GetCurrentTime(ITelegramBotClient bot, Message msg) {
        DateTime utcTime = DateTime.Now;
        await bot.SendMessage(
            chatId: msg.Chat,
            text: utcTime.ToString("U")
        );
    }
}