using System;
using Godot;


public static class MathM
{
    public static int BoolToInt(bool value) => value ? 1 : 0;
    /// <summary>
    /// Checks if two vectors are within a certain radius of each other.
    /// </summary>
    /// <param name="vecA">First vector.</param>
    /// <param name="vecB">Second vector.</param>
    /// <param name="radius">The maximum distance between the two vectors.</param>
    /// <returns>True if vecA and vecB are within radius of each other, otherwise false.</returns>
    public static bool IsInRange(Vector2 vecA, Vector2 vecB, float radius)
    {
        return vecA.DistanceTo(vecB) <= radius;
    }

    /// <summary>
    /// Returns instantaneous profit based on bell curve given byte progress and totalProfit (both 0–255).
    /// </summary>
    /// <param name="progress">Current progress (0–255).</param>
    /// <param name="totalProfit">Total profit to distribute (0–255).</param>
    /// <param name="spread">Spread of the bell curve in normalized units (default = 0.15).</param>
    /// <returns>Instantaneous profit value (can be fractional).</returns>
    public static int ProfitAtProgress(int progresso, int pico, int maxProgresso)
    {
        if (progresso > maxProgresso || progresso == 0) return 0;
        // 1 - 0
        // 2 - pico/4
        // 3 - pico/2
        // 4 - pico
        // 5 - pico/4
        // 6 - 0

        return (int)((pico * (float)progresso / maxProgresso) * 2) - 1;

        // return (int)(pico * (float)progresso / maxProgresso);

    }

    public static Vector2 RoundedVector(Vector2 vector) => new Vector2(Mathf.Round(vector.X), Mathf.Round(vector.Y));

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Mathf.Lerp(a.X, b.X, t), Mathf.Lerp(a.Y, b.Y, t));
    }

    public static Vector2 DegreeToVec2(float angle)
    {
        float radians = Mathf.DegToRad(angle);
        Vector2 vector = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        return vector;
    }

    

}
