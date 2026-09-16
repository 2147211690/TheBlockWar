namespace TheBlockWar.Scripts.Games.Onlines;

public interface IPlayerOperationSynchronizer
{
    void PushOperation(PlayerOperation operation);
    IAsyncEnumerable<PlayerOperation> GetAllOperationAsync();
}