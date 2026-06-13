namespace LabubuLog;

public static class ScoreDisplay
{
    public static string? HueStyle(double? score)
    {
        if (!score.HasValue)
        {
            return null;
        }

        var clampedScore = Math.Clamp(score.Value, 1, 10);
        var hue = (int)Math.Round((clampedScore - 1) / 9 * 120);

        return $"--score-hue:{hue};";
    }
}
