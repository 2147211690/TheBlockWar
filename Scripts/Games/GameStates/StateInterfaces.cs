namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyGameDataState<out T, out TData>
{
    TData ToData();
    T GetSnapshot();
}
public interface IGameDataState
{
    void ClearIfSnapshot();
}