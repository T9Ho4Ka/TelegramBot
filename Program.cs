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

async Task OnError(Exception exception, HandleErrorSource source) {
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}

async Task OnMessage(Message msg, UpdateType type) {
    stopwatch = Stopwatch.StartNew();
    
    if (msg.NewChatMembers != null) {
        if (msg.NewChatMembers.Any(user => user.IsBot && user.Id == bot.BotId)) {
            var chatId = msg.Chat.Id;
            Database.AddOrUpdateChat(chatId);
            Console.WriteLine($"[BOT] Бот добавлен в новый чат с ID: {chatId}");
        }
    }
    else if (msg.Text != null && msg.Text.StartsWith(Constants.Prefix)) {
        var request = msg.Text.Trim()[1..]; //pure request
        var mention = string.Empty;
        var flags = string.Empty;
        var isReply = false;
        var endOfCommand = request.IndexOf(' ');
        if (endOfCommand < 0) endOfCommand = request.Length; //if only the command was sent
        var command = request[..endOfCommand];
        
        var mentionEntity = msg.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.Mention);
        if (mentionEntity != null) mention = request.Substring(
                startIndex: mentionEntity.Offset,
                length: mentionEntity.Length-1);
        else if (msg.ReplyToMessage != null) isReply = true;
        
        await OnCommand(msg,command, mention, isReply, flags);
    }
    else if (type == UpdateType.Message) {
        Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat.Type}.  {msg.Chat.Username ?? msg.Chat.FirstName ?? msg.Chat.Title}");
    }
}
async Task OnCommand(Message msg, string command, string mention = "", bool isReply = false, string flags = "", string args = "") {
    try {
        if (!msg.From.IsBot) {
            Console.WriteLine($"Received command: {command} @{mention}, Is Reply: {isReply}, flags: {flags}");
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
                    await DataBaseManager.GetUserInfo(bot, msg, mention,isReply, flags);
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
    }finally { Response.LogResponseTime(stopwatch, bot, msg); }
}
