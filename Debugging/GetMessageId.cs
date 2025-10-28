namespace TelegramBot.Debugging;

public class GetMessageId
{
    public static async Task GetMsgId(ITelegramBotClient bot, Message msg) {
        await bot.SendMessage(
            chatId: msg.Chat,
            text: $"{msg.ReplyToMessage?.Id ?? msg.Id}");
    }
}