using Godot;

namespace TheBlockWar.Scripts.Games.Player;

public record struct PlayerInputClick(MouseButtonMask Button);
public record struct PlayerInputMousceCoords(Vector2I Coords);
