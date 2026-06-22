using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class TurnEvent(long id) : SchedulerEvent(id)
{
    public TypeEnum TurnType { get; set => SetField(ref field, value); }

    public override void OnApply(SchedulerEvent root, SimObject obj)
    {
        if (TurnType == TypeEnum.Region)
        {
            obj.ApplyRegionTurn(Turn);
        }
        else if (TurnType == TypeEnum.Buff)
        {
            obj.ApplyBuffTurn(Turn);
        }
    }

    public override bool CheckIsValid(IReadOnlyGameState gameState)
    {
        return true;
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> TurnEvent 类型:{TurnType}";
    }

    public enum TypeEnum
    {
        Buff,
        Region,
    }
}