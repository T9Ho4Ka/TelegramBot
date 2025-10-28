namespace TelegramBot.fun
{

    public class Ping
    {
        public static async Task Pong(ITelegramBotClient bot, Message msg) {
            
            // var httpClient = new HttpClient(); // Будущий функционал
            // string url = $"https://api.telegram.org/";
            //
            // var sw = Stopwatch.StartNew();
            // var response = await httpClient.GetAsync(url);
            // sw.Stop();
            var sw = Stopwatch.StartNew();
            await bot.SendMessage(
                chatId: msg.Chat.Id,
                text: "Pong",
                parseMode: ParseMode.Html);
            sw.Stop();
            var message = $"Отправлено запросов 1шт. Прошедшее время: {sw.ElapsedMilliseconds} мс.";
            await bot.SendMessage(
                chatId: msg.Chat.Id,
                text: message,
                ParseMode.Html
                );
        } 
    }
}