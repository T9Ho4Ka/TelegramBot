namespace TelegramBot.source;

public class UserStats {
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public int Level { get; set; } = 1;
    public float Exp { get; set; } = 0.0f;
    public long MessageCount { get; set; }
    public long RequiredExp { get; set; }
    
}

public class LeaderBoardEntry {
    public long UserId { get; set; }
    public int Rank { get; set; }
    public int Level { get; set; }
    public float Exp { get; set; }
    
}

public class Command{
    public string MainName { get;} //For example, mainName: .info | subNames .inf and .information 
    public List<string> CommandNames { get; set; } = new();
    public Command(string mainName) {
        MainName = mainName;
        CommandNames.Add(MainName);
    }
    public Command InitSubs(params string[] subNames) { 
        if (subNames != null) CommandNames.AddRange(subNames);
        return this;
    }


}
