namespace TheBlockWar.Scripts.Games.Onlines;

public class LocalPlayerOperationSynchronizer : IPlayerOperationSynchronizer
{
    private Queue<PlayerOperation> _operations = new();
    
    public void PushOperation(PlayerOperation operation)
    {
        _operations.Enqueue(operation);
    }

    public async IAsyncEnumerable<PlayerOperation> GetAllOperationAsync()
    {
        while (_operations.Count > 0)
        {
            yield return _operations.Dequeue();
        }
    }
}