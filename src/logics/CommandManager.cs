using TelegramBot.database;
using TelegramBot.fun;
using TelegramBot.general;

namespace TelegramBot.logics;

public class CommandManager {
    private static List<Command> CommandList = new();
    
    public static void CommandInit() {
        CommandList.Add(new Command("ping").InitSubs("p"));
        CommandList.Add(new Command("time").InitSubs("t"));
        CommandList.Add(new Command("information").InitSubs("info", "inf"));
        CommandList.Add(new Command("leaderboard").InitSubs("leaders", "leader", "top"));
    }

    private static string  HandleCommand(string command) {
        var cmd = CommandList.FirstOrDefault(c => c.CommandNames.Contains(command))?.MainName;
        return cmd;
    }
    
    public static async Task<bool> DispatchCommand(ITelegramBotClient bot, Message msg, string command, string mention, string args, List<string> flags, bool isReply, Stopwatch stopwatch) {
        try {
            var realCommand = HandleCommand(command);
            if (realCommand == null) return false;
            switch (realCommand) {
                case "ping":
                    await Ping.Pong(bot, msg);
                    break;
                case "time":
                    await Time.GetCurrentTime(bot, msg);
                    break;
                case "information":
                    await DataBaseManager.GetUserInfo(bot, msg, mention, flags, isReply);
                    break;
                case "leaderboard":
                    await DataBaseManager.GetLeaderBoard(bot, msg, args, flags);
                    break;
            }
        }
        finally {
            await Response.LogResponseTime(bot, msg, stopwatch); };
        return true;
    }
}