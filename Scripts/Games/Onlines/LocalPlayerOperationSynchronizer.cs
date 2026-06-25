namespace TheBlockWar.Scripts.Games.Onlines;

public class LocalPlayerOperationSynchronizer : IPlayerOperationSynchronizer
{
    private Queue<PlayerOperation> _operations = new();
    
    public void PushOperation(PlayerOperation operation)
    {
        _operations.Enqueue(operation);
    }

    public async GDTask<PlayerOperation> WaitOperationAsync()
    {
        await GDTask.WaitUntil(() => _operations.Count > 0);
        return _operations.Dequeue();
    }
    
}