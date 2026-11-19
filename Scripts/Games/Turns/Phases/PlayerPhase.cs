using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 玩家回合
/// </summary>
public class PlayerPhase : TurnPhase
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private int _team;
    public override string PhaseName { get; }
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Player;
    public override int Team => _team;
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
        _team = Player.Team;
    }

    public override async GDTask Execute()
    {
        Player.PhaseOrder = PhaseOrder;
        Player.CurrentTurn = Turn;
        Player.WaitConfirm = false;
        Player.TurnEnter = PlayerTurnEnterEnum.Free;
        
        Player.IsTurn = true;
        await foreach (var oper in OperationSynchronizer.GetAllOperationAsync())
        {
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
        if (operation is PlayerEndTurn)
        {
            return true;
        }
        else if (operation is PlayerUseUnitSkill useSkill)
        {
            SetAndRun(useSkill.UnitCoords, useSkill.SkillIndex, useSkill.Args, 0f);
        }
        else if (operation is PlayerPrepareUnitSkill prepareSkill)
        {
            await PrepareOnly(prepareSkill.UnitCoords, prepareSkill.SkillIndex, prepareSkill.Args, 0f);
        }
        else if (operation is PlayerSlowUseUnitSkill slowUseSkill)
        {
            SetAndRun(slowUseSkill.UnitCoords, slowUseSkill.SkillIndex, slowUseSkill.Args, slowUseSkill.UseDelay / 1000f);
        }
        else if (operation is PlayerSlowPrepareUnitSkill slowPrepareSkill)
        {
            await PrepareOnly(slowPrepareSkill.UnitCoords, slowPrepareSkill.SkillIndex, slowPrepareSkill.Args, 
                slowPrepareSkill.PrepareDelay / 1000f);
        }
        else if (operation is PlayerPlaceUnit placeUnit)
        {
            // TODO: 单位放置的新 API (ApplyPlaceUnit) 接入到事件调度器的写入口后实现
            Logger.Warn("PlayerPlaceUnit 暂未接新 API");
        }
        return false;

        // 立即设置技能并施放:写入 PrepareSkill 后运行
        void SetAndRun(Vector2I coords, int skillIndex, Vector2I?[]? args, float delay)
        {
            var unitId = ResolveUnitId(coords);
            if (unitId == 0 || !TryResolveSkill(coords, skillIndex, out var skillName)) return;
            EventScheduler.SetUnitPrepareSkill(unitId, new PrepareSkillInfo(skillName, args ?? []));
            EventScheduler.RunUnitPrepareSkill(unitId, delay);
        }

        // 仅预备下回合技能(设置 PrepareSkill)
        async GDTask PrepareOnly(Vector2I coords, int skillIndex, Vector2I?[]? args, float delay)
        {
            var unitId = ResolveUnitId(coords);
            if (unitId == 0 || !TryResolveSkill(coords, skillIndex, out var skillName)) return;
            EventScheduler.SetUnitPrepareSkill(unitId, new PrepareSkillInfo(skillName, args ?? []));
            EventScheduler.FocusedUnitId = unitId;
            await GDTask.Delay((int)(delay * 1000));
            EventScheduler.FocusedUnitId = 0;
        }

        long ResolveUnitId(Vector2I coords)
        {
            var unit = EventScheduler.GameState.UnitMapState.GetUnit(coords);
            return unit.IsNull() || unit.IsDead ? 0 : unit.Id;
        }

        bool TryResolveSkill(Vector2I coords, int skillIndex, out string skillName)
        {
            skillName = string.Empty;
            var unit = EventScheduler.GameState.UnitMapState.GetUnit(coords);
            if (unit.IsNull()) return false;
            var skills = EventScheduler.GameState.GetUnitValidSkills(unit.Name);
            if (skillIndex < 0 || skillIndex >= skills.Count) return false;
            skillName = skills[skillIndex].SkillName;
            return true;
        }
    }
}