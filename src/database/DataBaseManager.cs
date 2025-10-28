using TelegramBot.source;
namespace TelegramBot.database;

public class DataBaseManager {
    public static async Task AddExp(ITelegramBotClient bot, Message msg) {
        var rand = new Random();
        Console.WriteLine($"+ exp, @{msg.From.Username}");
        var Exp_ = (rand.Next(0, 500) / 100f);
        var stats = Database.GetChatStats(msg.From.Id, msg.Chat.Id) ??
                    Database.InitializeUser(msg.Chat.Id, msg.From.Id, msg.From.Username ?? msg.From.FirstName, Exp_);
        stats.MessageCount = ++stats.MessageCount;
        
        if (stats.Exp >= stats.RequiredExp){
            stats.Level = ++stats.Level;
            stats.Exp -= stats.RequiredExp;
            var requiredExp = Constants.RequiredExpForFirstLvl * (float)Math.Pow(stats.Level, 1.73f);
            stats.RequiredExp = (int)requiredExp;
            await bot.SendMessage(chatId: msg.Chat.Id, text: $"Поздравляю! Вы повысили уровень!");
        }
        Database.AddOrUpdateUserStats(stats);
    }
    public static async Task GetLeaderBoard(ITelegramBotClient bot, Message msg, string args, string flags) {
        var leaders = Database.GetLeaderBoard(msg.Chat.Id);
        if (leaders.Count == 0 || leaders == null) {
            await bot.SendMessage(chatId: msg.Chat.Id, text: "Нема лидерув", parseMode: ParseMode.Markdown);
            return;
        }
        var sb = new System.Text.StringBuilder($"🏆 <b><u>ТАБЛИЦА ЛИДЕРОВ</u></b>: 🏆\n");
        for (var i = 0; i < leaders.Count; i++) {
            var userStats = leaders[i];
            var user = await bot.GetChatMember(msg.Chat.Id, userStats.UserId);
            var username = user.User.Username != null 
                ? $"@{user.User.Username}" 
                : $"{user.User.FirstName}";
            sb.AppendLine($"<b>{i + 1}</b> [Name]: {username} | LVL: {userStats.Level} | EXP: {userStats.Exp.ToString("F2")} ");
        }
        await bot.SendMessage(chatId: msg.Chat.Id, text: sb.ToString(), parseMode: ParseMode.Html);
    }
    public static async Task GetUserInfo(ITelegramBotClient bot, Message msg, string mention, bool isReply, string flags) {
        var targetUser = msg.From;
        UserStats? userInfo = null;

        if (isReply) targetUser = msg.ReplyToMessage.From;
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

}