using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Games.Units;

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
[JsonDerivedType(typeof(ApplyPlaceTileStep), nameof(ApplyPlaceTileStep))]
[JsonDerivedType(typeof(ApplyPlaceUnitStep), nameof(ApplyPlaceUnitStep))]
[JsonDerivedType(typeof(ApplyPushUnitStep), nameof(ApplyPushUnitStep))]
[JsonDerivedType(typeof(ApplyRectWindStep), nameof(ApplyRectWindStep))]
[JsonDerivedType(typeof(ApplyStepsStep), nameof(ApplyStepsStep))]
public abstract record ApplyStep
{
    public abstract void Apply(SimObject obj);
}

public record ApplyAnimationStep(AnimationType Animation) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyAnimation(Animation);
    }
}

public record ApplyCureStep(Vector2I Coords, float Cure) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyCure(Coords, Cure);
    }
}

public record ApplyDamageStep(Vector2I Coords, float Damage) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyDamage(Coords, Damage);
    }
}

public record ApplyDeathStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyDeath(Coords);
    }
}

public record ApplyDelayStep(int Delay) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyDelay(Delay);
    }
}

public record ApplyDropTileStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyDropTile(Coords);
    }
}

public record ApplyDropUnitStep(Vector2I Coords) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyDropUnit(Coords);
    }
}

public record ApplyModifyTileRegionStep(Vector2I Coords, string RegionName, RegionOperation Operation, int Duration) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyModifyTileRegion(Coords, RegionName, Operation, Duration);
    }
}

public record ApplyModifyUnitBuffStep(Vector2I Coords, string BuffName, BuffOperation Operation, int Duration) : ApplyStep
{
    public override void Apply(SimObject obj)       
    {
        obj.ApplyModifyUnitBuff(Coords, BuffName, Operation, Duration);
    }
}

public record ApplyMoveUnitStep(Vector2I Coords, Vector2I To) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyMoveUnit(Coords, To);
    }
}

public record ApplyMoveUnitPathStep(Vector2I Coords, List<Vector2I>? Path) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyMoveUnitPath(Coords, Path);
    }
}

public record ApplyPlaceTileStep(Vector2I Coords, string TileName) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyPlaceTile(Coords, TileName);
    }
}

public record ApplyPlaceUnitStep(Vector2I Coords, string UnitName, int Team) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyPlaceUnit(Coords, UnitName, Team);
    }
}

public record ApplyPushUnitStep(Vector2I Coords, DirMask Dir) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplyPushUnit(Coords, Dir);
    }
}

public record ApplyRectWindStep(Rect2I Rect, DirMask Dir) : ApplyStep
{
    public override void Apply(SimObject obj)   
    {
        obj.ApplyRectWind(Rect, Dir);
    }
}

public record ApplyStepsStep(IEnumerable<ApplyStep> Applys, int StartDelay, int EndDelay, StepsRunningMode RunningMode) : ApplyStep
{
    public override void Apply(SimObject obj)
    {
        obj.ApplySteps(Applys, StartDelay, EndDelay, RunningMode);
    }
}
