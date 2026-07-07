using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Tools.Jsons;

namespace TheBlockWar.Scripts.Games.Onlines;


[JsonDerivedType(typeof(PlayerEndTurn), nameof(PlayerEndTurn))]
[JsonDerivedType(typeof(PlayerPrepareUnitSkill), nameof(PlayerPrepareUnitSkill))]
[JsonDerivedType(typeof(PlayerUseUnitSkill), nameof(PlayerUseUnitSkill))]
[JsonDerivedType(typeof(PlayerSlowUseUnitSkill), nameof(PlayerSlowUseUnitSkill))]
[JsonDerivedType(typeof(PlayerSlowPrepareUnitSkill), nameof(PlayerSlowPrepareUnitSkill))]

public record PlayerOperation(long PlayerId);
public record PlayerEndTurn(long PlayerId) : PlayerOperation(PlayerId);
public record PlayerPrepareUnitSkill(long PlayerId, Vector2I UnitCoords, int SkillIndex, Vector2I?[]? Args) : PlayerOperation(PlayerId);
public record PlayerUseUnitSkill(long PlayerId, Vector2I UnitCoords, int SkillIndex, Vector2I?[]? Args) : PlayerOperation(PlayerId);
public record PlayerSlowUseUnitSkill(long PlayerId, Vector2I UnitCoords, int SkillIndex, Vector2I?[]? Args, int UseDelay) : PlayerOperation(PlayerId);
public record PlayerSlowPrepareUnitSkill(long PlayerId, Vector2I UnitCoords, int SkillIndex, Vector2I?[]? Args, int PrepareDelay) : PlayerOperation(PlayerId);
public record PlayerPlaceUnit(long PlayerId, (Vector2I coords, string unitName)[] Places) : PlayerOperation(PlayerId);

[GlobalClass]
public partial class OnlinePlayerOperationSynchronizer : Node, IPlayerOperationSynchronizer
{
    private Queue<PlayerOperation> _operations = new();
    
    public void PushOperation(PlayerOperation operation)
    {
        var json = operation.ToJson();
        Rpc(MethodName.PushOperationRpc, json);
    }

    public async GDTask<PlayerOperation> WaitOperationAsync()
    {
        await GDTask.WaitUntil(() => _operations.Count > 0);
        return _operations.Dequeue();
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void PushOperationRpc(string operationJson)
    {
        var operation = operationJson.FromJson<PlayerOperation>()!;
        _operations.Enqueue(operation);
    }
}