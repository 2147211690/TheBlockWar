using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;

public class PlayerOperationHandler
{
    public bool IsTurn
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                _turnCompletionSource = new GDTaskCompletionSource();
                _turnCompletionSource.Task.Forget();
                Turn().Forget();
            }
            else
            {
                _turnCompletionSource?.TrySetResult();
                _turnCompletionSource = null;
            }
        }
    }
    public IPlayerOperationSynchronizer PlayerOperationSynchronizer { get; }

    public bool IsOperating
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                _operationCompletionSource = new GDTaskCompletionSource();
                _operationCompletionSource.Task.Forget();
            }
            else
            {
                _operationCompletionSource?.TrySetResult();
                _operationCompletionSource = null;
            }
        }
    }

    private GDTaskCompletionSource? _turnCompletionSource;
    private GDTaskCompletionSource? _operationCompletionSource;

    private IEventScheduler GameScheduler { get; } 
    private IPlayer Player { get;}
    
    public PlayerOperationHandler(IPlayerOperationSynchronizer playerOperationSynchronizer, IEventScheduler eventScheduler, IPlayer player)
    {
        PlayerOperationSynchronizer = playerOperationSynchronizer;
        GameScheduler = eventScheduler;
        Player = player;
    }

    private async GDTask Turn()
    {
        while (true)
        {
            var input = await PlayerOperationSynchronizer.WaitOperationAsync();
            IsOperating = true;
            if (input is PlayerEndTurn endTurn)
            {
                IsTurn = false;
                IsOperating = false;
                break;
            }
            else if (input is PlayerUseUnitSkill useSkill)
            {
                await UnitSkill(
                    useSkill.UnitCoords,
                    useSkill.SkillIndex, 
                    useSkill.Args, 
                    SimulationLayer.Running, 
                    Player.CurrentTurn);
                IsOperating = false;
            }
            else if (input is PlayerPrepareUnitSkill prepareSkill)
            {
                await UnitSkill(
                    prepareSkill.UnitCoords,
                    prepareSkill.SkillIndex,
                    prepareSkill.Args,
                    SimulationLayer.Waiting,
                    Player.CurrentTurn + 1);
                IsOperating = false;
            }
            else if (input is PlayerSlowUseUnitSkill slowUseSkill)
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
                    await GameScheduler.WaitCompleteRunning();
                }
                IsOperating = false;
            }
            else if (input is PlayerSlowPrepareUnitSkill slowPrepareSkill)
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
                IsOperating = false;
            }
        }

        async GDTask<SchedulerEvent?> UnitSkill(Vector2I coords, int skillIndex, Vector2I?[]? args, SimulationLayer layer, int turn)
        {
            var unit = GameScheduler.GameState.UnitMapState.GetUnit(coords);
            if (unit.IsNull() || unit.IsDead)
            {
                return null;
            }
            var skills = GameScheduler.GameState.GetUnitValidSkills(unit.Name);
            if (skillIndex < 0 || skillIndex >= skills.Count)
            {
                return null;
            }
            var skill = skills[skillIndex];
            var e = GameScheduler.CreateEvent(id => new SkillEvent(id)
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
            await GameScheduler.WaitCompleteRunning();
            return e;
        }
    }
    public GDTask WaitTurnComplete() => _turnCompletionSource?.Task ?? GDTask.CompletedTask;
    public GDTask WaitOperationComplete() => _operationCompletionSource?.Task ?? GDTask.CompletedTask;
}