using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Units;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

public record SimStep(int Deep)
{
    public static readonly SimStep Default = new(0);
}
public record DelayStep(int Deep, int Delay) : SimStep(Deep);
public record DamageStep(int Deep, Vector2I Coords,float Damage, long ResultUnitId, float ResultValidDamage) : SimStep(Deep);
public record CureStep(int Deep, Vector2I Coords,float Cure, long ResultUnitId, float ResultValidCure) : SimStep(Deep);
public record DeathStep(int Deep, Vector2I Coords, long ResultUnitId, bool ResultIsValid) : SimStep(Deep);
public record PlaceTileStep(int Deep, Vector2I Coords, TileState Tile) : SimStep(Deep);
public record DropTileStep(int Deep, Vector2I Coords) : SimStep(Deep);
public record PlaceUnitStep(int Deep, Vector2I Coords, string UnitName, int Team, UnitState ResultUnit, bool ResultIsValid) : SimStep(Deep);
public record MoveUnitStep(int Deep, Vector2I Coords, Vector2I To, long ResultUnitId, bool ResultIsValid) : SimStep(Deep);
public record MoveUnitPathStep(int Deep, Vector2I Coords, List<Vector2I> Path, long ResultUnitId, int ResultTargetIndex ,bool ResultIsValid) : SimStep(Deep);
public record DropUnitStep(int Deep, Vector2I Coords, long ResultUnitId, bool ResultIsValid) : SimStep(Deep);
public record PushUnitStep(int Deep, Vector2I Coords, DirMask PushDir ,long ResultUnitId, bool ResultPush) : SimStep(Deep);
public record RectWindStep(int Deep, Rect2I Rect, DirMask Dir) : SimStep(Deep);
public record StepsStep(int Deep, int StartDelay, int EndDelay, StepsRunningMode RunningMode) : SimStep(Deep);


/// <param name="ResultOperation">只有AddBuff,RemoveBuff,ChangeDuration结果</param>
public record ModifyUnitBuffStep(int Deep, Vector2I Coords, string BuffName, BuffOperation Operation, int Duration,
    long ResultUnitId, BuffOperation ResultOperation, int ResultDuration) : SimStep(Deep);
public record ModifyTileRegionStep(int Deep, Vector2I Coords, string RegionName, RegionOperation Operation, int Duration,
    RegionOperation ResultOperation, int ResultDuration) : SimStep(Deep);
public record AnimationStep(int Deep, AnimationType Animation) : SimStep(Deep);

//Animations
[JsonDerivedType(typeof(ShotSomethingAnim), typeDiscriminator: nameof(ShotSomethingAnim))]
[JsonDerivedType(typeof(ThrowSomethingAnim), typeDiscriminator: nameof(ThrowSomethingAnim))]
[JsonDerivedType(typeof(UnitJumpToAnim), typeDiscriminator: nameof(UnitJumpToAnim))]
[JsonDerivedType(typeof(SummonEffectAnim), typeDiscriminator: nameof(SummonEffectAnim))]
public record AnimationType();
public record ShotSomethingAnim(Vector2I Coords, Vector2I To, string ThingName, float Time) : AnimationType;
public record ThrowSomethingAnim(Vector2I Coords, Vector2I To, string ThingName, float ThrowHeight, float Time) : AnimationType;
public record UnitJumpToAnim(Vector2I Coords, Vector2I To, float Time) : AnimationType;
public record SummonEffectAnim(Vector2I Coords, string EffectName) : AnimationType;
[Flags]
public enum BuffOperation
{
    None,
    AddBuff,
    RemoveBuff,
    ChangeDuration,
    SetDuration
}
[Flags]
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
