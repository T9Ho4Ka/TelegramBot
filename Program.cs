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

string? GetToken()
{
    
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddUserSecrets<Program>()
        .Build();

    string botToken = configuration["Bot:Token"];

    if (string.IsNullOrEmpty(botToken))
    {
        Console.WriteLine("Ошибка: Переменная окружения не найдена.");
        return "YOUR_BOT_TOKEN";
    }
    return botToken;
}

var botToken = GetToken() ?? "YOUR_BOT_TOKEN";
using var cts = new CancellationTokenSource();

var bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
var botStatus = await bot.GetMe();

bot.OnError += OnError;
bot.OnMessage += OnMessage;

var startedTest =
    $"Bot is running." +
    $"\n=========INFO=========" +
    $"\nUserName: {botStatus.Username}" +
    $"\nUserId: {botStatus.Id}" +
    $"\nClient version: {Constants.VERSION}" +
    $"\nPress Enter to terminate" +
    $"\n======================";

Console.WriteLine(startedTest); 
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
