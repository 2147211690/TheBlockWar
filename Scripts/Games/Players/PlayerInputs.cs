namespace TheBlockWar.Scripts.Games.Players;

public record struct PlayerInputClick(Vector2I Coords, PlayerClickTypeEnum Type);
public record struct PlayerInputMouseCoords(Vector2I Coords);
public record struct PlayerInputStopTurn(PlayerSotpTurnEnum Type);
public record struct PlayerInputSkill(int Index);

public enum PlayerSotpTurnEnum
{
    HoverEnter,
    HoverExit,
    Click,
}
public enum PlayerClickTypeEnum
{
    Left,
    Right,
}