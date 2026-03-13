using Godot;

namespace TheBlockWar.Scripts.Games.Players;

public record struct PlayerInputClick(Vector2I Coords, MouseButtonMask Button);
public record struct PlayerInputMousceCoords(Vector2I Coords);
