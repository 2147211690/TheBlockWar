namespace TheBlockWar.Scripts.Games.Onlines;

public interface IPlayerOperationSynchronizer
{
    void PushOperation(PlayerOperation operation);
    GDTask<PlayerOperation> WaitOperationAsync();
}