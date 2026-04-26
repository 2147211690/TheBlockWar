using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

[JsonDerivedType(typeof(ApplyAnimationStep), nameof(ApplyAnimationStep))]
[JsonDerivedType(typeof(ApplyCureStep), nameof(ApplyCureStep))]
[JsonDerivedType(typeof(ApplyDamageStep), nameof(ApplyDamageStep))]
[JsonDerivedType(typeof(ApplyDeathStep), nameof(ApplyDeathStep))]
[JsonDerivedType(typeof(ApplyDelayStep), nameof(ApplyDelayStep))]
[JsonDerivedType(typeof(ApplyDropTileStep), nameof(ApplyDropTileStep))]
[JsonDerivedType(typeof(ApplyDropUnitStep), nameof(ApplyDropUnitStep))]
[JsonDerivedType(typeof(ApplyModifyTileRegionStep), nameof(ApplyModifyTileRegionStep))]
[JsonDerivedType(typeof(ApplyModifyUnitBuffStep), nameof(ApplyModifyUnitBuffStep))]
[JsonDerivedType(typeof(ApplyMoveUnitStep), nameof(ApplyMoveUnitStep))]
[JsonDerivedType(typeof(ApplyMoveUnitPathStep), nameof(ApplyMoveUnitPathStep))]
[JsonDerivedType(typeof(ApplyNextParallelStep), nameof(ApplyNextParallelStep))]
[JsonDerivedType(typeof(ApplyPlaceTileStep), nameof(ApplyPlaceTileStep))]
[JsonDerivedType(typeof(ApplyPlaceUnitStep), nameof(ApplyPlaceUnitStep))]
[JsonDerivedType(typeof(ApplyPushUnitStep), nameof(ApplyPushUnitStep))]
[JsonDerivedType(typeof(ApplyStepsStep), nameof(ApplyStepsStep))]
public abstract record ApplyStep
{
    public abstract void Apply(IGameState state, SimulationContext context);
}

public record ApplyAnimationStep(AnimationType Animation) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyAnimation(Animation, context);
    }
}

public record ApplyCureStep(Vector2I Coords, float Cure) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyCure(Coords, Cure, context);
    }
}

public record ApplyDamageStep(Vector2I Coords, float Damage) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyDamage(Coords, Damage, context);
    }
}

public record ApplyDeathStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyDeath(Coords, context);
    }
}

public record ApplyDelayStep(int Delay) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyDelay(Delay, context);
    }
}

public record ApplyDropTileStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyDropTile(Coords, context);
    }
}

public record ApplyDropUnitStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyDropUnit(Coords, context);
    }
}

public record ApplyModifyTileRegionStep(Vector2I Coords, RegionTypeEnum RegionType, RegionOperation Operation, int Duration) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyModifyTileRegion(Coords, RegionType, Operation, Duration, context);
    }
}

public record ApplyModifyUnitBuffStep(Vector2I Coords, BuffTypeEnum BuffType, BuffOperation Operation, int Duration) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyModifyUnitBuff(Coords, BuffType, Operation, Duration, context);
    }
}

public record ApplyMoveUnitStep(Vector2I Coords, Vector2I To) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyMoveUnit(Coords, To, context);
    }
}

public record ApplyMoveUnitPathStep(Vector2I Coords, List<Vector2I>? Path) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyMoveUnitPath(Coords, Path, context);
    }
}

public record ApplyNextParallelStep() : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyNextParallel(context);
    }
}

public record ApplyPlaceTileStep(Vector2I Coords, TileState Tile) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyPlaceTile(Coords, Tile, context);
    }
}

public record ApplyPlaceUnitStep(Vector2I Coords, string UnitName, int Team, UnitState? PrefabUnit) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyPlaceUnit(Coords, UnitName, Team, PrefabUnit, context);
    }
}

public record ApplyPushUnitStep(Vector2I Coords, DirMask Dir) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplyPushUnit(Coords, Dir, context);
    }
}

public record ApplyStepsStep(ApplyStep[] Applys) : ApplyStep
{
    public override void Apply(IGameState state, SimulationContext context)
    {
        state.ApplySteps(Applys, context);
    }
}
