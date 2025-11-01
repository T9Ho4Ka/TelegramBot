using TelegramBot.source;
namespace TelegramBot.database;

public class DataBaseManager {
    public static async Task AddExp(ITelegramBotClient bot, Message msg) {
        var rand = new Random();
        var genExp = (rand.Next(0, 500) / 100f);
        var stats = Database.GetChatStats(chatId: msg.Chat.Id, userId: msg.From.Id) ??
                    Database.InitializeUser(msg.Chat.Id, msg.From.Id, msg.From.Username ?? msg.From.FirstName, genExp);
        stats.MessageCount = ++stats.MessageCount;
        if (stats.Exp >= stats.RequiredExp){ //level up
            stats.Level = ++stats.Level;
            stats.Exp -= stats.RequiredExp;
            var requiredExp = Constants.RequiredExpForFirstLvl * (float)Math.Pow(stats.Level, 1.73f);
            stats.RequiredExp = (int)requiredExp;
            await bot.SendMessage(chatId: msg.Chat.Id, text: $"Поздравляю! {stats.UserName}. Вы повысили уровень!");
        }
        Database.AddOrUpdateUserStats(stats);
    }
    public static async Task GetLeaderBoard(ITelegramBotClient bot, Message msg, string args, List<string> flags) {
        var leaders = Database.GetLeaderBoard(msg.Chat.Id);
        if (leaders.Count == 0) {
            await bot.SendMessage(chatId: msg.Chat.Id, text: "Нема лидерув", parseMode: ParseMode.Html);
            return;
        }
        var sb = new System.Text.StringBuilder($"🏆 <b><u>ТАБЛИЦА ЛИДЕРОВ</u></b>: 🏆\n");
        for (var i = 0; i < leaders.Count; i++) {
            var userStats = leaders[i];
            sb.AppendLine($"<b>{i + 1}</b> [Name]: {userStats.UserName} | LVL: {userStats.Level} | EXP: {userStats.Exp.ToString("F2")} ");
        }
        await bot.SendMessage(chatId: msg.Chat.Id, text: sb.ToString(), parseMode: ParseMode.Html);
    }
    public static async Task GetUserInfo(ITelegramBotClient bot, Message msg, string mention, List<string> flags, bool isReply) {
        var targetUser = msg.From;
        UserStats? userInfo = new();
        if (isReply) targetUser = msg.ReplyToMessage?.From;
        else if (mention != string.Empty) {
            userInfo = Database.GetChatStatsByName(chatId: msg.Chat.Id, userName: mention);
            if (userInfo == null)
                await bot.SendMessage(msg.Chat.Id,
                    "Пользователь ещё не зарегистрирован в системе. Ему нужно написать хотя бы 1 сообщение или команду в чат.");
        }

        if (mention == string.Empty) userInfo = Database.GetChatStats(chatId: msg.Chat.Id, userId: targetUser.Id);
        await bot.SendMessage(
            chatId: msg.Chat.Id,
            text: $"<b>Info user`s {userInfo.UserName}</b>\n" +
                  $"Level: {userInfo.Level}\n" +
                  $"Exp: {userInfo.Exp.ToString("F2")}\n" +
                  $"Required exp: {userInfo.RequiredExp}\n" +
                  $"A total of {userInfo.MessageCount} messages have been posted",
            parseMode: ParseMode.Html);

    }

    public static async Task AddGroupChat(long chatId) {
        Database.AddOrUpdateChat(chatId); 
        Console.WriteLine($"[BOT] Бот добавлен в новую группу с ID: {chatId}");
    }

}