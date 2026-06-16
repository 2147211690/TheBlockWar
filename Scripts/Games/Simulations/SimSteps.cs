using System.Text.Json.Serialization;

namespace TheBlockWar.Scripts.Games.Simulations;

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
    AddBuff,
    RemoveBuff,
}
public enum BuffOperationResult
{
    None,
    BuffAdded,
    BuffRemoved,
    BuffCountChanged,
}
public enum RegionOperation
{
    AddRegion,
    RemoveRegion,
}
public enum RegionOperationResult
{
    None,
    RegionAdded,
    RegionRemoved,
    RegionDurationChanged,
}

public enum StepsRunningMode
{
    QueueWaitAll,
    QueueWithoutWait,
    ParallelWaitAny,
    ParallelWaitAll,
    ParallelWithoutWait,
}