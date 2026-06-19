using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Tools;
/// <summary>
/// 通用动画辅助工具。
///
/// 坐标系统说明（GamePosition）：
///   Vector3 (x, y, z) 中：
///     - x, y = 方格坐标（整数值=方格中心）
///     - z    = 高度（游戏单位），TilesView.NormalGroundHeight 定义了一个方格平面距离对应的 z 高度
///
/// 所有位置类方法要求 target 实现 IGameViewNode，通过 GamePosition 属性驱动。
/// 视觉类方法（颜色、缩放）使用 Godot 标准属性。
/// </summary>
public static class AnimationHelper
{
    // ========================================================================
    //  1. 下落动画（基于 GamePosition.z）
    // ========================================================================

    /// <summary>
    /// 简易弹跳下落。将 target 的 GamePosition.z 抬升后弹跳回落。
    /// 适用于 TileView 等轻量节点。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="gameDropHeight">抬升高度（z 游戏单位）</param>
    /// <param name="duration">单次坠落时长（秒）</param>
    public static GDTask DropInAsync<T>(T target, float gameDropHeight = 1f, float duration = 0.03f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        var groundPos = target.GamePosition;
        target.GamePosition = groundPos with { Z = groundPos.Z + gameDropHeight };

        var tween = target.CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(target, "GamePosition", groundPos, duration);
        return tween.WaitForTween();
    }

    /// <summary>
    /// 重型下落 + 撞击压扁 + 回弹恢复。GamePosition.z 驱动位置，squashSprite 的 local Scale 驱动形变。
    /// 适用于 UnitView 等重量级节点。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D（整个单位节点）</param>
    /// <param name="squashSprite">压扁效果作用到的子节点（如 MainSprite），传 null 则压扁 target 自身</param>
    /// <param name="gameDropHeight">抬升高度（z 游戏单位）</param>
    /// <param name="fallDuration">加速下落时长（秒）</param>
    /// <param name="impactSquashY">撞击时 Y 轴压扁倍数（>1 视觉上压扁）</param>
    /// <param name="impactStretchX">撞击时 X 轴拉伸倍数</param>
    /// <param name="recoveryDuration">回弹恢复时长（秒）</param>
    public static GDTask DropInHeavyAsync<T>(T target,
        float gameDropHeight = 1.5f,
        float fallDuration = 0.1f,
        float impactSquashY = 1.3f,
        float impactStretchX = 0.25f,
        float recoveryDuration = 0.04f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        var groundPos = target.GamePosition;
        target.GamePosition = groundPos with { Z = groundPos.Z + gameDropHeight };

        target.Scale = Vector2.One;

        var tween = target.CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.In);

        // 阶段1：重型坠落（GamePosition.z 加速下降）
        var midPos = groundPos with { Z = groundPos.Z + 0.3f };
        tween.TweenProperty(target, "GamePosition", midPos, fallDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);

        // 阶段2：撞击地面（压扁 + 最终到位）
        tween.Chain()
            .TweenProperty(target, "scale",
                new Vector2(impactStretchX, impactSquashY), 0.02f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.Parallel()
            .TweenProperty(target, "GamePosition", groundPos, 0.02f);

        // 阶段3：回弹恢复
        tween.Chain()
            .TweenProperty(target, "scale", Vector2.One, recoveryDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        return tween.WaitForTween();
    }

    // ========================================================================
    //  2. 受击动画：GamePosition 小范围抖动 + 闪白
    // ========================================================================

    /// <summary>
    /// 受击效果：GamePosition 在 XY 平面随机偏移抖动 + 周期性闪白。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="shakeRadius">抖动半径（游戏坐标单位，0.1 ≈ 1/10 格）</param>
    /// <param name="shakeCount">抖动次数</param>
    /// <param name="flashCount">闪白次数</param>
    /// <param name="flashIntensity">闪白强度（>1 表示过曝）</param>
    public static GDTask HurtShakeAsync<T>(T target,
        float shakeRadius = 0.1f,
        int shakeCount = 4,
        int flashCount = 3,
        float flashIntensity = 3f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        var originPos = target.GamePosition;
        var originColor = target.Modulate;
        var tween = target.CreateTween();
        tween.SetParallel();

        // 随机方向抖动（GamePosition xy 偏移）
        for (int i = 0; i < shakeCount; i++)
        {
            var r = GD.Randf() * Mathf.Pi * 2;
            var offset = new Vector3(
                Mathf.Cos(r) * shakeRadius,
                Mathf.Sin(r) * shakeRadius,
                0);
            tween.TweenProperty(target, "GamePosition", originPos + offset, 0)
                .SetTrans(Tween.TransitionType.Linear)
                .SetDelay(i * 0.03f);
        }

        // 闪白
        for (int i = 0; i < flashCount; i++)
        {
            tween.TweenProperty(target, "modulate",
                    new Color(flashIntensity, flashIntensity, flashIntensity, 1f), 0.02f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetDelay(i * 0.03f);
            tween.TweenProperty(target, "modulate", originColor, 0.02f)
                .SetTrans(Tween.TransitionType.Linear);
        }

        return tween.WaitForTween(t =>
        {
            target.GamePosition = originPos;
            target.Modulate = originColor;
        });
    }

    // ========================================================================
    //  3. 跳跃/位移动画（基于 GamePosition Vector3）
    // ========================================================================

    /// <summary>
    /// 抛物线跳跃：从 fromPos 到 toPos，带弧高。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="fromPos">起始 GamePosition</param>
    /// <param name="toPos">终点 GamePosition</param>
    /// <param name="gameArcHeight">弧高（z 游戏单位，正值向上）</param>
    /// <param name="duration">跳跃时长（秒）</param>
    public static async GDTask JumpToAsync<T>(T target,
        Vector3 fromPos, Vector3 toPos,
        float gameArcHeight = 1.5f, float duration = 0.3f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        target.GamePosition = fromPos;
        var startTime = Time.GetTicksMsec();
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed = (Time.GetTicksMsec() - startTime) / 1000f;
            var t = Mathf.Clamp(elapsed / duration, 0f, 1f);

            // XY 平面线性插值，Z 抛物线弧高
            var pos = fromPos.Lerp(toPos, t);
            pos.Z += Mathf.Sin(t * Mathf.Pi) * gameArcHeight;

            target.GamePosition = pos;
            await GDTask.Yield();
        }

        target.GamePosition = toPos;
    }

    /// <summary>
    /// 沿路径逐段移动（每段 GamePosition Tween）。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="path">路径点序列（Vector3，GamePosition）</param>
    /// <param name="speedPerSegment">每段移动时长（秒）</param>
    public static async GDTask MoveAlongPathAsync<T>(T target,
        IReadOnlyList<Vector3> path, float speedPerSegment = 0.15f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        if (path.Count == 0) return;

        target.GamePosition = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            var tween = target.CreateTween();
            tween.TweenProperty(target, "GamePosition", path[i], speedPerSegment)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);
            await tween.WaitForTween();
        }
    }

    /// <summary>
    /// 贝塞尔曲线弧线移动（二次贝塞尔，Z 方向为弧高）。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="fromPos">起始 GamePosition</param>
    /// <param name="toPos">终点 GamePosition</param>
    /// <param name="gameArcHeight">弧高（z 游戏单位）</param>
    /// <param name="duration">时长（秒）</param>
    public static async GDTask BezierMoveAsync<T>(T target,
        Vector3 fromPos, Vector3 toPos,
        float gameArcHeight, float duration)
        where T : Node2D, IGameViewNode<Node2D>
    {
        target.GamePosition = fromPos;
        var center = fromPos.Lerp(toPos, 0.5f) with { Z = fromPos.Z + gameArcHeight };

        var startTime = Time.GetTicksMsec();
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed = (Time.GetTicksMsec() - startTime) / 1000f;
            var t = Mathf.Clamp(elapsed / duration, 0f, 1f);

            var l1 = fromPos.Lerp(center, t);
            var l2 = center.Lerp(toPos, t);
            target.GamePosition = l1.Lerp(l2, t);

            await GDTask.Yield();
        }

        target.GamePosition = toPos;
    }

    // ========================================================================
    //  4. 脉冲/呼吸效果（视觉属性：Scale）
    // ========================================================================

    /// <summary>
    /// 缩放脉冲动画：在 1 → scaleTo → 1 之间来回缩放。
    /// </summary>
    public static GDTask PulseAsync(Node2D target,
        Vector2 scaleTo, float duration = 0.3f, int loops = 1)
    {
        var tween = target.CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.InOut);

        if (loops == -1)
            tween.SetLoops();
        else if (loops > 1)
            tween.SetLoops(loops * 2);

        tween.TweenProperty(target, "scale", scaleTo, duration);
        tween.TweenProperty(target, "scale", Vector2.One, duration);

        return tween.WaitForTween();
    }

    /// <summary>
    /// 单次呼吸脉冲（放大 → 恢复）。
    /// </summary>
    public static GDTask BreathPulseAsync(Node2D target,
        Vector2 peakScale, float duration = 0.3f)
        => PulseAsync(target, peakScale, duration, 1);

    // ========================================================================
    //  5. 淡入/淡出（视觉属性：Modulate）
    // ========================================================================

    /// <summary>
    /// 淡入（Modulate 从 Transparent → White）。
    /// </summary>
    public static GDTask FadeInAsync(Node2D target, float duration = 0.3f)
    {
        target.Modulate = Colors.Transparent;
        var tween = target.CreateTween();
        tween.TweenProperty(target, "modulate", Colors.White, duration);
        return tween.WaitForTween();
    }

    /// <summary>
    /// 淡出（Modulate 从当前 → Transparent）。
    /// </summary>
    public static GDTask FadeOutAsync(Node2D target, float duration = 0.3f)
    {
        var tween = target.CreateTween();
        tween.TweenProperty(target, "modulate", Colors.Transparent, duration);
        return tween.WaitForTween();
    }

    // ========================================================================
    //  6. 组合动画
    // ========================================================================

    /// <summary>
    /// 淡入 + GamePosition.z 下落 的组合入场动画。
    /// </summary>
    /// <param name="target">实现了 IGameViewNode 的 Node2D</param>
    /// <param name="gameDropHeight">抬升高度（z 游戏单位）</param>
    /// <param name="fadeDuration">淡入 + 下落时长（秒）</param>
    public static async GDTask FadeInDropAsync<T>(T target,
        float gameDropHeight = 1f, float fadeDuration = 0.15f)
        where T : Node2D, IGameViewNode<Node2D>
    {
        var groundPos = target.GamePosition;
        target.GamePosition = groundPos with { Z = groundPos.Z + gameDropHeight };
        target.Modulate = Colors.Transparent;

        var tween = target.CreateTween();
        tween.SetParallel();
        tween.TweenProperty(target, "GamePosition", groundPos, fadeDuration)
            .SetTrans(Tween.TransitionType.Bounce)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(target, "modulate", Colors.White, fadeDuration);

        await tween.WaitForTween();
    }

    // ========================================================================
    //  7. 工具方法
    // ========================================================================

    /// <summary>
    /// 创建一个 Tween 并快速配置常用参数。
    /// </summary>
    public static Tween CreateTween(Node2D target,
        Tween.TransitionType trans = Tween.TransitionType.Quad,
        Tween.EaseType ease = Tween.EaseType.InOut,
        bool parallel = false,
        float speedScale = 1f)
    {
        var tween = target.CreateTween();
        tween.SetTrans(trans);
        tween.SetEase(ease);
        tween.SetSpeedScale(speedScale);
        if (parallel) tween.SetParallel();
        return tween;
    }
}
