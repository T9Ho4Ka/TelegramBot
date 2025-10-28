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
