using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;

public class PlayerPhase : ITurnPhase
{
    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; } = null!;
    public TurnManager TurnManager { get; private set; } = null!;
    public IPlayer Player { get; }
    private IPlayerOperationSynchronizer _operationSynchronizer;
    public PlayerPhase(IPlayerOperationSynchronizer operationSynchronizer, IPlayer player)
    {
        _operationSynchronizer = operationSynchronizer;
        Player = player;
    }
    public ITurnPhase Init(TurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        Player.TurnManager = turnManager;
        return this;
    }
    public async GDTask Execute()
    {
        Player.PhaseOrder = PhaseOrder;
        Player.CurrentTurn = Turn;
        Player.IsTurn = true;
        Player.WaitConfirm = false;
        while (true)
        {
            var oper = await _operationSynchronizer.WaitOperationAsync();
            if (oper.PlayerId != Player.Id) continue;
            Player.WaitConfirm = true;
            if (await Handle(oper))
            {
                Player.IsTurn = false;
                Player.WaitConfirm = false;
                break;
            }
            Player.WaitConfirm = false;
        }
    }

    /// <summary>
    /// 处理玩家操作
    /// </summary>
    /// <param name="operation"></param>
    /// <returns>是否应该结束处理</returns>
    public async GDTask<bool> Handle(PlayerOperation operation)
    {
        if (operation is PlayerEndTurn endTurn)
        {
            return true;
        }
        else if (operation is PlayerUseUnitSkill useSkill)
        {
            await UnitSkill(
                useSkill.UnitCoords,
                useSkill.SkillIndex,
                useSkill.Args,
                SimulationLayer.Running,
                Player.CurrentTurn);
        }
        else if (operation is PlayerPrepareUnitSkill prepareSkill)
        {
            await UnitSkill(
                prepareSkill.UnitCoords,
                prepareSkill.SkillIndex,
                prepareSkill.Args,
                SimulationLayer.Waiting,
                Player.CurrentTurn + 1);
        }
        else if (operation is PlayerSlowUseUnitSkill slowUseSkill)
        {
            var e = await UnitSkill(
                slowUseSkill.UnitCoords,
                slowUseSkill.SkillIndex,
                slowUseSkill.Args,
                SimulationLayer.Focused,
                Player.CurrentTurn + 1);
            if (e != null)
            {
                await GDTask.Delay(slowUseSkill.UseDelay);
                e.Layer = SimulationLayer.Running;
                await EventScheduler.WaitCompleteRunning();
            }
        }
        else if (operation is PlayerSlowPrepareUnitSkill slowPrepareSkill)
        {
            var e = await UnitSkill(
                slowPrepareSkill.UnitCoords,
                slowPrepareSkill.SkillIndex,
                slowPrepareSkill.Args,
                SimulationLayer.Focused,
                Player.CurrentTurn + 1);
            if (e != null)
            {
                await GDTask.Delay(slowPrepareSkill.PrepareDelay);
                e.Layer = SimulationLayer.Waiting;
            }
        }
        return false;

        async GDTask<SchedulerEvent?> UnitSkill(Vector2I coords, int skillIndex, Vector2I?[]? args,
            SimulationLayer layer, int turn)
        {
            var unit = EventScheduler.GameState.UnitMapState.GetUnit(coords);
            if (unit.IsNull() || unit.IsDead)
            {
                return null;
            }

            var skills = EventScheduler.GameState.GetUnitValidSkills(unit.Name);
            if (skillIndex < 0 || skillIndex >= skills.Count)
            {
                return null;
            }

            var skill = skills[skillIndex];
            var e = EventScheduler.CreateEvent(id => new SkillEvent(id)
            {
                Layer = layer,
                PhaseOrder = Player.PhaseOrder,
                Priority = 0,
                SkillArgs = args,
                SkillBindUnitId = unit.Id,
                SkillName = skill.SkillName,
                Team = unit.Team,
                Turn = turn,
            });
            await EventScheduler.WaitCompleteRunning();
            return e;
        }
    }
}