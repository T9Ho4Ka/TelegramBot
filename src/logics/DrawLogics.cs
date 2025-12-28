using SkiaSharp;
namespace TelegramBot.logics;

public class DrawLogics {
    
    public static void IsExampleExit() {
        if (!File.Exists(Constants.DefaultTemplatePath)) GenerateDefaultTemplate();
        else Console.WriteLine("\ufe0f Successful! Найден базовый экземпляр карты");
        if (!File.Exists($"{Constants.AvatarsPath}/default.jpg")) {} //download from GitHub
    }
    private static void GenerateDefaultTemplate() => GenerateNewTemplate(0,0,0);
    private static void GenerateNewTemplate(int R = 0, int G = 0, int B = 0,  int width = 800, int height = 300) {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        using var paint = new SKPaint {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(40,200),
                new SKPoint(width, height),
                new[] { new SKColor(50, 40, 100), new SKColor(20, 15, 40) },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, width, height)), paint);
        using var borderPaint = new SKPaint {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            Color = new SKColor(100, 90, 180)
        };
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(0, 0, width, height)), borderPaint);

        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Jpeg, 200);
        File.WriteAllBytes(Constants.DefaultTemplatePath, data.ToArray());
    }
    
    public static byte[] GenerateCard(string username, int level, float currentExp, int maxExp, long userId) {
        using var template = SKBitmap.Decode(Constants.DefaultTemplatePath);
        using var surface = SKSurface.Create(new SKImageInfo(template.Width, template.Height));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(template, 0, 0);

        // === Аватарка ===
        using var avatar = SKBitmap.Decode($"{Constants.AvatarsPath}/avatar{userId}.jpg");
        int avatarSize = 128;
        var avatarRect = new SKRect(50, (template.Height - avatarSize) / 2, 50 + avatarSize, (template.Height + avatarSize) / 2);
        canvas.Save();

        using (var path = new SKPath()) {
            path.AddOval(avatarRect);
            canvas.ClipPath(path, antialias: true);
            canvas.DrawBitmap(avatar, avatarRect);
        }

        canvas.Restore();

        using (var paint = new SKPaint {
                   IsAntialias = true,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = 5,
                   Color = new SKColor(120, 180, 255)
               }) {
            canvas.DrawOval(avatarRect, paint);
        }

        // === Имя пользователя ===
        using (var textPaint = new SKPaint {
                   Color = SKColors.White,
                   TextSize = 36,
                   IsAntialias = true,
                   Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
               }) {
            canvas.DrawText(username, 200, 130, textPaint);
        }

        // === Level ===
        using (var levelPaint = new SKPaint {
                   Color = new SKColor(180, 200, 255),
                   TextSize = 28,
                   IsAntialias = true
               }) {
            canvas.DrawText($"Level {level}", 200, 175, levelPaint);
        }

        // === EXP Bar ===
        float expPercent = Math.Clamp((float)currentExp / maxExp, 0, 1);
        var barX = 200f;
        var barY = 200f;
        var barW = 500f;
        var barH = 28f;

        using (var bg = new SKPaint { Color = new SKColor(50, 50, 80), IsAntialias = true })
        using (var fg = new SKPaint {
                   Shader = SKShader.CreateLinearGradient(
                       new SKPoint(barX, barY),
                       new SKPoint(barX + barW, barY),
                       new[] { new SKColor(120, 150, 255), new SKColor(70, 100, 220) },
                       null,
                       SKShaderTileMode.Clamp),
                   IsAntialias = true
               }) {
            var radius = barH / 2;
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(barX, barY, barX + barW, barY + barH), radius, radius), bg);
            canvas.DrawRoundRect(
                new SKRoundRect(new SKRect(barX, barY, barX + barW * expPercent, barY + barH), radius, radius), fg);
        }

        // === Текст EXP ===
        using (var expText = new SKPaint {
                   Color = new SKColor(200, 210, 255),
                   TextSize = 20,
                   IsAntialias = true
               }) {
            string expString = $"{currentExp.ToString("F2")} / {maxExp} EXP";
            var textWidth = expText.MeasureText(expString);
            canvas.DrawText(expString, barX + barW - textWidth, barY - 5, expText);
        }

        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
} 