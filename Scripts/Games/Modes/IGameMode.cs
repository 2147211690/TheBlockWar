using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public interface IGameMode
{
    GameStateEnum State { get; }
    void EnterTurn(ITurnManager turnManager, int turn);
    GDTask Turn(ITurnManager turnManager, int turn);
    void ExitTurn(ITurnManager turnManager, int turn);
}

public enum GameStateEnum
{
    Playing,
    Paused,
    Ended
}