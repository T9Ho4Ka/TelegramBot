using Microsoft.Data.Sqlite;
namespace TelegramBot.database;

public static class Database {
    public const string DbFileName = "database.db";
    private static readonly string DatabasePath = GetSolutionRootPath(DbFileName);
    private static string ConnectionString => $"Data Source={DatabasePath}";
    private static string GetSolutionRootPath(string dbFileName) {
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDir != null && !currentDir.GetFiles("*.sln").Any() && currentDir.Parent != null) currentDir = currentDir.Parent;
        
        string path = currentDir != null && currentDir.GetFiles("*.sln").Any() 
                      ? currentDir.FullName 
                      : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        
        return Path.Combine(path, dbFileName);
    }
    public static void InitializeDatabase() {
        bool isNewDatabase = !File.Exists(DatabasePath);
        try {
            using (var connection = new SqliteConnection(ConnectionString)) {
                connection.Open();
                Console.WriteLine($"[INFO] Соединение с БД установлено. Путь: {DatabasePath}");

                if (!isNewDatabase) return;
                CreateTables(connection);
                Console.WriteLine("[INFO] Новая база данных инициализирована и таблицы созданы.");
            }
        }
        catch (Exception ex){ Console.WriteLine($"[ERROR] Критическая ошибка работы с БД: {ex.Message}"); }
}
    private static void CreateTables(SqliteConnection connection) {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            -- Таблица 1: Chat
            CREATE TABLE IF NOT EXISTS Chat (
                ChatId INTEGER PRIMARY KEY, 
                IsPremium INTEGER NOT NULL DEFAULT 0 );

            -- Таблица 2: Users
            CREATE TABLE IF NOT EXISTS Users (
                ChatId INTEGER, 
                UserId INTEGER,
                UserName TEXT,
                Level INTEGER NOT NULL DEFAULT 0,
                Exp REAL NOT NULL DEFAULT 0.0,
                MessageCount INTEGER,
                RequiredExp INTEGER,
                
                PRIMARY KEY (ChatId, UserId),
                FOREIGN KEY (ChatId) REFERENCES Chat(ChatId) 
                    ON DELETE CASCADE);";
        command.ExecuteNonQuery();
    }
    public static void AddOrUpdateChat(long ChatId, bool IsPremium = false) {
        const string sql = @"
            -- Добавляет, если нет. Игнорирует, если есть.
            INSERT OR IGNORE INTO Chat (ChatId, IsPremium) 
            VALUES (@ChatId, @IsPremium);
            
            -- Обновляет независимо от того, был ли INSERT
            UPDATE Chat SET IsPremium = @IsPremium
            WHERE ChatId = @ChatId;
        ";

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        command.Parameters.AddWithValue("@ChatId", ChatId);
        command.Parameters.AddWithValue("@IsPremium", IsPremium);
        
        command.ExecuteNonQuery();
    }
    public static void AddOrUpdateUserStats(UserStats stats) {
        const string sql = @"
            -- .
            INSERT OR REPLACE INTO Users (ChatId, UserId, UserName, Level, Exp, MessageCount, RequiredExp) 
            VALUES (@ChatId, @UserId, @UserName, @Level, @Exp, @MessageCount, @RequiredExp);";
        
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        command.Parameters.AddWithValue("@ChatId", stats.ChatId);
        command.Parameters.AddWithValue("@UserId", stats.UserId);
        command.Parameters.AddWithValue("@UserName", stats.UserName);
        command.Parameters.AddWithValue("@Level", stats.Level);
        command.Parameters.AddWithValue("@Exp", stats.Exp);
        command.Parameters.AddWithValue("@MessageCount", stats.MessageCount);
        command.Parameters.AddWithValue("@RequiredExp", stats.RequiredExp);

        command.ExecuteNonQuery();
    }

    public static UserStats InitializeUser(long chatId, long userId, string userName, float exp = 0.0f) {
        var user = new UserStats {
            ChatId = chatId,
            UserId = userId,
            UserName = userName,
            Exp = exp,
            Level = 0,
            RequiredExp = (int)Constants.RequiredExpForFirstLvl,
            MessageCount = 0
        };
        AddOrUpdateUserStats(user);
        return user;
    }
    public static UserStats? GetChatStats(long userId, long chatId) {
        const string sql = "SELECT UserName, Level, Exp, MessageCount, RequiredExp FROM Users WHERE ChatId = @ChatId AND UserId = @UserId;";
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ChatId", chatId);

        using var reader = command.ExecuteReader();
        if (reader.Read()) {
            return new UserStats {
                UserId = userId,
                ChatId = chatId,
                UserName = reader.GetString(0),
                Level = reader.GetInt32(1),
                Exp = reader.GetFloat(2),
                MessageCount = reader.GetInt32(3),
                RequiredExp = reader.GetInt32(4)
            };
        }
        return null;
    }
    public static UserStats? GetChatStatsByName(long chatId, string userName) {
        const string sql = "SELECT UserId, UserName, Level, Exp, MessageCount, RequiredExp FROM Users WHERE ChatId = @ChatId AND UserName = @UserName;";
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@UserName", userName.TrimStart('@'));
        using var reader = command.ExecuteReader();
        if (reader.Read()) {
            return new UserStats {
                ChatId = chatId,
                UserId = reader.GetInt64(0),
                UserName = reader.GetString(1),
                Level = reader.GetInt32(2),
                Exp = reader.GetFloat(3),
                MessageCount = reader.GetInt32(4),
                RequiredExp = reader.GetInt32(5)
            };
        }

        return null;
    }
    public static List<UserStats> GetLeaderBoard(long chatId, int limit = 10) {
        const string sql = @"
              -- .
              SELECT UserId, Level, Exp
              FROM Users
              WHERE ChatId = @ChatId
              ORDER BY Level DESC, Exp DESC 
                  LIMIT @Limit";
        var leaderboard = new List<UserStats>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@Limit", limit);
        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            leaderboard.Add(new UserStats {
                UserId = reader.GetInt64(0),
                Level = reader.GetInt32(1),
                Exp = reader.GetFloat(2)
            });
        }

        return leaderboard;
    }
}