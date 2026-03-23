using GodotTask;

namespace TheBlockWar.Scripts.Games.Players;

public interface IPlayer
{
    GameScheduler? GameScheduler { get; set; }
    GDTask Complete { get; }
    void OnInput<T>(in T input);
    void Init();
}