using System.Data;
using Microsoft.Extensions.Configuration;
using TelegramBot.Debugging;
using TelegramBot.general;
using TelegramBot.fun;
using TelegramBot.database;
using TelegramBot.logics;

Stopwatch stopwatch;
Database.InitializeDatabase();
CommandManager.CommandInit();
var bot = TokenManager.InitTokenAsync();

using var cts = new CancellationTokenSource(); 
var botStatus = await bot.GetMe(cts.Token);
var startedTest =
    $"""
     Bot is running.
     =========INFO=========
     UserName: {botStatus.Username}
     UserId: {botStatus.Id}
     Client version: {Constants.VERSION}
     Press Enter to terminate
     ======================
     """;
Console.WriteLine(startedTest);

bot.OnError += OnError;
bot.OnMessage += OnMessage;

Console.ReadLine();
cts.Cancel(); // stop the bot
async Task OnMessage(Message msg, UpdateType type) {
    stopwatch = Stopwatch.StartNew();
    if(msg.Chat.Type == ChatType.Private) return;// and more
    
     if (msg.Text != null && msg.Text.StartsWith(Constants.Prefix) && msg.Text.Length is > 2 and < 50) { 
         Parser.ParseRequest(ref msg, out var command, out var mention, out var args, out var flags, out var isReply);
        await OnCommand(msg, command, mention, args, flags, isReply);
     }else if (msg.NewChatMembers != null)
         if (msg.NewChatMembers.Any(user => user.IsBot && user.Id == bot.BotId))
             await DataBaseManager.AddGroupChat(msg.Chat.Id);
         else await OnCommand(msg, "","", "",new(), false);
}
async Task OnCommand(Message msg, string command, string mention, string args, List<string> flags, bool isReply) {
        if (msg.From is{ IsBot: false}) { //If the bot sent a message | ignore

            CommandManager.DispatchCommand(bot,msg, command,mention,args,flags,isReply, stopwatch);
            if (!Constants.IsCommandsConsidered && command == string.Empty) {
                await DataBaseManager.AddExp(bot, msg);
            }
        }
        else await bot.SendMessage(msg.Chat.Id, "эй ноу, brother. You are fucking bot");
}

async Task OnError(Exception exception, HandleErrorSource source) {
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
} 
