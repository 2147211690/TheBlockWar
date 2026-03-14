using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TheBlockWar.Scripts.Tools;

/// <summary>
/// Godot 原生 Vector2I 格子导航类
/// 支持：最短路径、范围消耗寻路、可经过但不可作为终点、完全委托驱动
/// </summary>
public class GridNavigator
{
    // ====================== 核心委托（外部传入地图逻辑） ======================
    /// <summary> 格子是否可以经过 </summary>
    private Func<Vector2I, bool> _canPass;

    /// <summary> 格子移动消耗 </summary>
    private Func<Vector2I, int> _getMoveCost;

    /// <summary> 格子是否可以作为【最终落脚点】 </summary>
    private Func<Vector2I, bool> _canBeDestination;

    // 四方向移动（斜视角游戏常用）
    private readonly Vector2I[] _directions =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
    };

    // ====================== 构造 ======================
    public GridNavigator(Func<Vector2I, bool> canPass, Func<Vector2I, int> getMoveCost, Func<Vector2I, bool> canBeDestination)
    {
        _canPass = canPass ?? throw new ArgumentNullException(nameof(canPass));
        _getMoveCost = getMoveCost ?? throw new ArgumentNullException(nameof(getMoveCost));
        _canBeDestination = canBeDestination ?? throw new ArgumentNullException(nameof(canBeDestination));
    }

    // ====================== 1. 两点之间最短路径 ======================
    /// <summary>
    /// 寻找起点到终点的最短路径（带消耗）
    /// 终点必须满足：可经过 + 可作为最终点
    /// </summary>
    public List<Vector2I> FindShortestPath(Vector2I start, Vector2I end)
    {
        if (!_canPass(end) || !_canBeDestination(end))
            return null;

        if (start == end)
            return new List<Vector2I> { start };

        var dist = new Dictionary<Vector2I, int>();
        var prev = new Dictionary<Vector2I, Vector2I>();
        var queue = new PriorityQueue<Vector2I, int>();

        dist[start] = 0;
        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            queue.TryDequeue(out var current, out var currentCost);

            if (current == end)
                return BuildPath(prev, current);

            if (dist.TryGetValue(current, out var knownCost) && knownCost < currentCost)
                continue;

            foreach (var dir in _directions)
            {
                var next = current + dir;

                if (!_canPass(next))
                    continue;

                var cost = _getMoveCost(next);
                var newCost = currentCost + cost;

                if (!dist.ContainsKey(next) || newCost < dist[next])
                {
                    dist[next] = newCost;
                    prev[next] = current;
                    queue.Enqueue(next, newCost);
                }
            }
        }

        return null;
    }

    // ====================== 2. 指定消耗内所有可达格子（包括可经过不可落脚） ======================
    public Dictionary<Vector2I, int> FindAllReachableWithinCost(Vector2I start, int maxCost)
    {
        var result = new Dictionary<Vector2I, int>();
        if (!_canPass(start)) return result;

        var queue = new Queue<(Vector2I pos, int cost)>();
        queue.Enqueue((start, 0));
        result[start] = 0;

        while (queue.Count > 0)
        {
            var (current, currentCost) = queue.Dequeue();

            foreach (var dir in _directions)
            {
                var next = current + dir;
                if (!_canPass(next)) continue;

                var addCost = _getMoveCost(next);
                var newCost = currentCost + addCost;

                if (newCost > maxCost) continue;
                if (result.TryGetValue(next, out var existingCost) && existingCost <= newCost)
                    continue;

                result[next] = newCost;
                queue.Enqueue((next, newCost));
            }
        }

        return result;
    }

    // ====================== 3. 指定消耗内【可落脚】的所有终点 ======================
    public List<Vector2I> FindValidDestinationsWithinCost(Vector2I start, int maxCost)
    {
        return FindAllReachableWithinCost(start, maxCost)
            .Where(kvp => _canBeDestination(kvp.Key))
            .OrderBy(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    // ====================== 工具：反向构建路径 ======================
    private List<Vector2I> BuildPath(Dictionary<Vector2I, Vector2I> prev, Vector2I end)
    {
        var path = new List<Vector2I>();
        var current = end;

        while (prev.ContainsKey(current))
        {
            path.Add(current);
            current = prev[current];
        }

        path.Add(current);
        path.Reverse();
        return path;
    }
}