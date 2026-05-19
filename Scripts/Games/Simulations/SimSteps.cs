using System.Text.Json.Serialization;

[JsonDerivedType(typeof(ShotSomethingAnim), typeDiscriminator: nameof(ShotSomethingAnim))]
[JsonDerivedType(typeof(ThrowSomethingAnim), typeDiscriminator: nameof(ThrowSomethingAnim))]
[JsonDerivedType(typeof(UnitJumpToAnim), typeDiscriminator: nameof(UnitJumpToAnim))]
[JsonDerivedType(typeof(SummonEffectAnim), typeDiscriminator: nameof(SummonEffectAnim))]
public record AnimationType();
public record ShotSomethingAnim(Vector2I Coords, Vector2I To, string ThingName, float Time) : AnimationType;
public record ThrowSomethingAnim(Vector2I Coords, Vector2I To, string ThingName, float ThrowHeight, float Time) : AnimationType;
public record UnitJumpToAnim(Vector2I Coords, Vector2I To, float Time) : AnimationType;
public record SummonEffectAnim(Vector2I Coords, string EffectName) : AnimationType;
public enum BuffOperation
{
    None,
    AddBuff,
    RemoveBuff,
    ChangeDuration,
    SetDuration
}
public enum RegionOperation
{
    None,
    AddRegion,
    RemoveRegion,
    ChangeDuration,
    SetDuration
}

public enum StepsRunningMode
{
    QueueWaitAll,
    QueueWithoutWait,
    ParallelWaitAny,
    ParallelWaitAll,
    ParallelWithoutWait,
}
