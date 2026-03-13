namespace TheBlockWar.Scripts.Games.Players;

public interface IPlayer
{
    GameScheduler? GameScheduler { get; set; }
    void OnInput<T>(in T input);
    void Init();
}