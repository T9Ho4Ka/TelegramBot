using System.Data;
using Microsoft.Extensions.Configuration;
using TelegramBot.Debugging;
using TelegramBot.general;
using TelegramBot.fun;
using TelegramBot.database;
using Constants = TelegramBot.source.Constants;

Stopwatch stopwatch;
Database.InitializeDatabase();

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
        ParseRequest(ref msg, out var request, out var command, out var mention, out var args, out var flags, out var isReply);
        await OnCommand(msg, command, mention, args, flags, isReply);
     }else if (msg.NewChatMembers != null)
         if (msg.NewChatMembers.Any(user => user.IsBot && user.Id == bot.BotId))
             await DataBaseManager.AddGroupChat(msg.Chat.Id);
         else await OnCommand(msg, "","", "",new(), false);
}
async Task OnCommand(Message msg, string command, string mention, string args, List<string> flags, bool isReply) {
    try {
        if (msg.From is{ IsBot: false}) { //If the bot sent a message | ignore
            //not necessarily --------- start
            string allMyFlags = " ";
            int jff = flags.Count;
            for (int j = 0; j < jff ; j++) {
                allMyFlags += flags[j];
            }
            var text = $"Original request: {msg.Text}\ncommand: {command}\nmention: {mention}\nargs: {args}\nisReply: {isReply}\nflags {allMyFlags} ";
            Console.WriteLine($"Received command: \n{text}");
            //not necessarily ----------- end
            switch (command) {
                case $"ping":
                    await Ping.Pong(bot, msg);
                    break;
                case $"time":
                    await Time.GetCurrentTime(bot, msg);
                    break;
                case $"leaders":
                    await DataBaseManager.GetLeaderBoard(bot, msg, args, flags);
                    break;
                case $"info":
                    await DataBaseManager.GetUserInfo(bot, msg, mention, flags, isReply);
                    break;
            }

            if (!Constants.IsCommandsConsidered && command == string.Empty) {
                await DataBaseManager.AddExp(bot, msg);
            }
        }
        else {
            await bot.SendMessage(
                msg.Chat.Id,
                "эй ноу, brother. You are fucking bot");
        }
    }finally { await Response.LogResponseTime(stopwatch, bot, msg); }
}

async Task OnError(Exception exception, HandleErrorSource source) {
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}

void ParseRequest(ref Message msg,
    out string request,
    out string command,
    out string mention,
    out string args,
    out List<string> flags, out bool isReply) {
    
    mention = string.Empty;
    args = string.Empty;
    flags = new();
    isReply = msg.ReplyToMessage != null;
    request = msg.Text?.Trim()[1..] ?? ""; //trimmed request 
    //==============
    //   Command
    //==============
    var endOfCommand = request.IndexOf(' ');
    if (endOfCommand < 0) {
        endOfCommand = request.Length;
        command = request[..endOfCommand];
        return;
    }
     //if only the command was sent
    command = request[..endOfCommand];
    request = request[(command.Length + 1)..].Trim(); //-command


    //==============
    //   Mention
    //==============
    var mentionEntity = msg.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.Mention);
    if (mentionEntity != null) {
        mention = msg.Text.Substring(
            startIndex: mentionEntity.Offset,
            length: mentionEntity.Length);
        request = request.Replace(mention, "").Trim();
    }
    //else -> me
    if (string.IsNullOrEmpty(request)) return; // command mention.
    
    //==============
    //   flags
    //==============
    while (true){
        var flagIndex = request.IndexOf(Constants.OptionPrefix);
        if (flagIndex < 0) break; //если флагов нет.
    
        var flagEnd = request.IndexOf(' ', flagIndex);
        if (flagEnd < 0) flagEnd = request.Length; // and break
        
        var flag = request.Substring(flagIndex, flagEnd - flagIndex);
        if (flag.Length == 2) flags.Add(flag);
        request = request.Remove(flagIndex, flag.Length).Trim();
    }
    if (string.IsNullOrEmpty(request)) return; //command flag/command mention flag
    
    //==============
    //   args
    //==============
    
    var isArgs = request.IndexOf(' '); // the end args
    if (isArgs > 0) args = request[..(isArgs+1)];
    else args = request;
}