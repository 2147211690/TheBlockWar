using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Tools;

public static class CoordsRegion
{
    public static IEnumerable<Vector2I> GetSquareRegion(Vector2I center, int range)
    {
        for (var x = -range; x <= range; x++)
        for (var y = -range; y <= range; y++)
            yield return new Vector2I(center.X + x, center.Y + y);
    }

    public static IEnumerable<Vector2I> GetDiamondRegion(Vector2I center, int range)
    {
        for (var x = -range; x <= range; x++)
        {
            var yRange = range - Math.Abs(x);
            for (var y = -yRange; y <= yRange; y++) yield return new Vector2I(center.X + x, center.Y + y);
        }
    }

    public static IEnumerable<Vector2I> GetLineRegion(Vector2I start, Vector2I end)
    {
        var delta = end - start;
        var steps = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));
        if (steps == 0)
        {
            yield return start;
            yield break;
        }

        var step = new Vector2I(delta.X / steps, delta.Y / steps);
        var current = start;
        for (var i = 0; i <= steps; i++)
        {
            yield return current;
            current += step;
        }
    }

    public static IEnumerable<Vector2I> GetCircleRegion(Vector2I center, int radius)
    {
        var rsq = radius * radius;
        for (var x = -radius; x <= radius; x++)
        for (var y = -radius; y <= radius; y++)
            if (x * x + y * y <= rsq)
                yield return new Vector2I(center.X + x, center.Y + y);
    }

    public static IEnumerable<Vector2I> GetRingRegion(Vector2I center, int innerRadius, int outerRadius)
    {
        var innerRsq = innerRadius * innerRadius;
        var outerRsq = outerRadius * outerRadius;
        for (var x = -outerRadius; x <= outerRadius; x++)
        for (var y = -outerRadius; y <= outerRadius; y++)
        {
            var distSq = x * x + y * y;
            if (distSq <= outerRsq && distSq >= innerRsq) yield return new Vector2I(center.X + x, center.Y + y);
        }
    }

    /// <summary>
    ///     获取从start到end的连续路径(上下左右)区域，包含start和end
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static IEnumerable<Vector2I> GetPathRegion(Vector2I start, Vector2I end)
    {
        var current = start;
        yield return current;
        while (current != end)
        {
            if (current.X < end.X)
                current.X++;
            else if (current.X > end.X)
                current.X--;
            else if (current.Y < end.Y)
                current.Y++;
            else if (current.Y > end.Y)
                current.Y--;
            yield return current;
        }
    }
}