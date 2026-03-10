using Godot;

namespace TheBlockWar.Scripts.Games;

public record SimStep { }

public record DamageStep(Vector2I Coords, float Damage, uint ResultUnitId, float ResultValidDamage) : SimStep;
public record DeathStep(Vector2I Coords, uint ResultUnitId, bool ResultIsValid) : SimStep;
public record PlaceTileStep(Vector2I Coords, TileState Tile) : SimStep;
public record DropTileStep(Vector2I Coords) : SimStep;
public record PlaceUnitStep(Vector2I Coords, UnitState Unit, uint ResultUnitId, bool ResultIsValid) : SimStep;
public record MoveUnitStep(Vector2I Coords, Vector2I To, uint ResultUnitId, bool ResultIsValid) : SimStep;
