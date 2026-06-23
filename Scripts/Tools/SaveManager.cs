using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Tools;

public static class SaveManager
{
    public static GameStateData SaveCurrentGameState(IReadOnlyGameState gameState)
    {
        var data = gameState.ToData();
        return data;
    }

    public static void LoadCurrentGameState(GameStateData data, IGameState gameState, IEventScheduler? eventScheduler)
    {
        gameState.LoadGameData(data);
        eventScheduler?.ReloadGameState();
    }
}