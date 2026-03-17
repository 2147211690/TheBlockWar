using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public record SimStep(bool IsChildStep)
{
    public static readonly SimStep Default = new(false);
}

public record ParallelStep(bool IsChildStep, bool IsStart) : SimStep(IsChildStep);
public record DamageStep(bool IsChildStep, Vector2I Coords,float Damage, uint ResultUnitId, float ResultValidDamage) : SimStep(IsChildStep);
public record DeathStep(bool IsChildStep, Vector2I Coords, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record PlaceTileStep(bool IsChildStep, Vector2I Coords, TileState Tile) : SimStep(IsChildStep);
public record DropTileStep(bool IsChildStep, Vector2I Coords) : SimStep(IsChildStep);
public record PlaceUnitStep(bool IsChildStep, Vector2I Coords, UnitState? Unit, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record MoveUnitStep(bool IsChildStep, Vector2I Coords, Vector2I To, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record MoveUnitPathStep(bool IsChildStep, Vector2I Coords, List<Vector2I> Path, uint ResultUnitId, int ResultTargetIndex ,bool ResultIsValid) : SimStep(IsChildStep);
public record DropUnitStep(bool IsChildStep, Vector2I Coords, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record PushUnitStep(bool IsChildStep, Vector2I Coords, DirMask PushDir ,uint ResultUnitId, bool ResultPush) : SimStep(IsChildStep);
/// <param name="ResultOperation">只有AddBuff,RemoveBuff,ChangeDuration结果</param>
public record ModifyUnitBuffStep(bool IsChildStep, Vector2I Coords, BuffTypeEnum BuffType, BuffOperation Operation, int Duration,
    uint ResultUnitId, BuffOperation ResultOperation, int ResultDuration) : SimStep(IsChildStep);
public record ModifyTileRegionStep(bool IsChildStep, Vector2I Coords, RegionTypeEnum RegionType, RegionOperation Operation, int Duration,
    RegionOperation ResultOperation, int ResultDuration) : SimStep(IsChildStep);
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