using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public interface IGameMode
{
    GameStateEnum State { get; }
    void EnterTurn(TurnManager turnManager, int turn);
    GDTask Turn(TurnManager turnManager, int turn);
    void ExitTurn(TurnManager turnManager, int turn);
}

public enum GameStateEnum
{
    Playing,
    Paused,
    Ended
}