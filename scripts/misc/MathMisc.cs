using System;
using Godot;


public static class MathM
{
    public static int BoolToInt(bool value) => value? 1 : 0;
    /// <summary>
    /// Checks if two vectors are within a certain radius of each other.
    /// </summary>
    /// <param name="vecA">First vector.</param>
    /// <param name="vecB">Second vector.</param>
    /// <param name="radius">The maximum distance between the two vectors.</param>
    /// <returns>True if vecA and vecB are within radius of each other, otherwise false.</returns>
    public static bool IsInRange(Vector2 vecA, Vector2 vecB, float radius){
        return vecA.DistanceTo(vecB) <= radius;
    }

}
