namespace TheBlockWar.Scripts.Games.Player;

public interface IPlayer
{
    GameScheduler? GameScheduler { get; set; }
    void OnInput<T>(in T input);
    void Init();
}