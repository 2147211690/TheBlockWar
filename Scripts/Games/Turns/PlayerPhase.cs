using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 玩家回合
/// </summary>
public class PlayerPhase : TurnPhase
{
    public override string PhaseName { get; }
    public override bool IsFocused { get; set; }
    public IPlayer Player { get; }
    protected IPlayerOperationSynchronizer OperationSynchronizer { get; }
    public PlayerPhase(IPlayerOperationSynchronizer operationSynchronizer, IPlayer player)
    {
        OperationSynchronizer = operationSynchronizer;
        Player = player;
        PhaseName = $"Player{Player.Team}Phase";
    }
    protected override void OnInit()
    {
        Player.TurnManager = TurnManager;
    }
    public override async GDTask Execute()
    {
        Player.PhaseOrder = PhaseOrder;
        Player.CurrentTurn = Turn;
        Player.IsTurn = true;
        Player.WaitConfirm = false;
        while (true)
        {
            var oper = await OperationSynchronizer.WaitOperationAsync();
            if (oper.PlayerId != Player.Id) continue;
            Player.WaitConfirm = true;
            if (await HandleNext(oper))
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
    public async GDTask<bool> HandleNext(PlayerOperation operation)
    {
        if (operation is PlayerEndTurn endTurn)
        {
            return true;
        }
        else if (operation is PlayerUseUnitSkill useSkill)
        {
            var e = UnitSkill(
                useSkill.UnitCoords,
                useSkill.SkillIndex,
                useSkill.Args,
                SimulationLayer.Disabled,
                Player.CurrentTurn);
            e?.Layer = SimulationLayer.Running;
            await EventScheduler.WaitCompleteRunning();
        }
        else if (operation is PlayerPrepareUnitSkill prepareSkill)
        {
            var e = UnitSkill(
                prepareSkill.UnitCoords,
                prepareSkill.SkillIndex,
                prepareSkill.Args,
                SimulationLayer.Waiting,
                Player.CurrentTurn + 1);
        }
        else if (operation is PlayerSlowUseUnitSkill slowUseSkill)
        {
            var e = UnitSkill(
                slowUseSkill.UnitCoords,
                slowUseSkill.SkillIndex,
                slowUseSkill.Args,
                SimulationLayer.Focused,
                Player.CurrentTurn + 1);
            if (e != null)
            {
                await GDTask.Delay(slowUseSkill.UseDelay);
                e.Layer = SimulationLayer.Disabled;
                e.Layer = SimulationLayer.Running;
                await EventScheduler.WaitCompleteRunning();
            }
        }
        else if (operation is PlayerSlowPrepareUnitSkill slowPrepareSkill)
        {
            var e = UnitSkill(
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
        else if (operation is PlayerPlaceUnit placeUnit)
        {
            foreach (var place in placeUnit.Places)
            { }
        }
        return false;

        EventState? UnitSkill(Vector2I coords, int skillIndex, Vector2I?[]? args,
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
            var e = EventScheduler.CreateEvent(new SkillEventCore(skill.SkillName, unit.Id, args ?? []),
                EventScheduler.PhaseOrders[Turn + 1][PhaseName], unit.Team, layer);
            return e;
        }
    }
}