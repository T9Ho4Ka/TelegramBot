namespace TelegramBot.general;

public class Status
{
    public static async Task status(ITelegramBotClient bot, Message msg, Stopwatch stopwatch) { //В разработке.
            await bot.EditMessageText(
                chatId: msg.Chat,
                messageId: 205,
                text: $"Coming soon."
            );
    }
}