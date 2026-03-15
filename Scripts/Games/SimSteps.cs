using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public record SimStep(bool IsChildStep)
{
    public static readonly SimStep Default = new(false);
}
public record DamageStep(bool IsChildStep, Vector2I Coords,float Damage, uint ResultUnitId, float ResultValidDamage) : SimStep(IsChildStep);
public record DeathStep(bool IsChildStep, Vector2I Coords, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record PlaceTileStep(bool IsChildStep, Vector2I Coords, TileState Tile) : SimStep(IsChildStep);
public record DropTileStep(bool IsChildStep, Vector2I Coords) : SimStep(IsChildStep);
public record PlaceUnitStep(bool IsChildStep, Vector2I Coords, UnitState? Unit, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record MoveUnitStep(bool IsChildStep, Vector2I Coords, Vector2I To, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record MoveUnitPathStep(bool IsChildStep, Vector2I Coords, List<Vector2I> Path, uint ResultUnitId, int ResultTargetIndex ,bool ResultIsValid) : SimStep(IsChildStep);
public record DropUnitStep(bool IsChildStep, Vector2I Coords, uint ResultUnitId, bool ResultIsValid) : SimStep(IsChildStep);
public record PushUnitStep(bool IsChildStep, Vector2I Coords, DirMask PushDir ,uint ResultUnitId, bool ResultPush) : SimStep(IsChildStep);
