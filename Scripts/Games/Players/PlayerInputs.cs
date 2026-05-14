namespace TheBlockWar.Scripts.Games.Players;

public record struct PlayerInputClick(Vector2I Coords, MouseButtonMask Button);
public record struct PlayerInputMouseCoords(Vector2I Coords);
public record struct PlayerInputStopTurn(PlayerSotpTurnEnum Type);
public record struct PlayerInputSkill(int Index);

public enum PlayerSotpTurnEnum
{
    HoverEnter,
    HoverExit,
    Click,
}