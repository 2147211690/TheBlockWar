using System.ComponentModel;
using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;

public class EasyAiPlayer : IPlayer
{
    private IPlayerOperationSynchronizer _playerOperationSynchronizer;
    public EasyAiPlayer(long id, IEventScheduler gameScheduler, IPlayerOperationSynchronizer playerOperationSynchronizer)
    {
        Id = id;
        _playerOperationSynchronizer = playerOperationSynchronizer;
        GameScheduler = gameScheduler;
    }
    public long Id { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public IEventScheduler? GameScheduler { get; set; }
    public long SelectedUnitId { get; }
    public int Power { get; }
    public int Team { get; set; }

    public bool IsTurn
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            if (value)
            {
                TurnAsync().Forget();
            }
        }
    }

    public PlayerTurnEnterEnum TurnEnter { get; set; }

    public ITurnManager TurnManager { get; set; }

    public int PhaseOrder { get; set; }
    public int CurrentTurn { get; set; }
    public int SelectedSkillIndex { get; set; }
    public Vector2I? SelectedTileCoords { get; set; }
    public bool WaitConfirm { get; set; } = true;

    public void OnInput<T>(in T input)
    {
    }

    public void Init()
    {
    }
    
    public async GDTask TurnAsync()
    {
        var state = GameScheduler!.GameState;
        var units = state.UnitMapState.GetAllUnits().Where(u => u.Team == Team && !u.IsDead).ToList();
        var nextTurn = GameScheduler.CurrentTurn + 1;

        foreach (var u in units)
        {
            if (GameScheduler.GameState.EventManagerState.GetUnitEvents(u.Id).Count > 0)
                continue;

            var skills = state.GetUnitValidSkills(u.Name);
            var validSkills = skills.Where(s => u.GetShouldUseSkill(state, s)).ToList();
            if (validSkills.Count == 0)
                continue;

            var skillIndex = Random.Shared.Next(validSkills.Count);
            var skill = validSkills[skillIndex];
            var args = new Vector2I?[skill.ArgCount];
            var success = true;
            for (var i = 0; i < skill.ArgCount; i++)
            {
                var validArgs = skill.GetSkillArgs(state, u, u.Coords, i, args).ToList();
                if (validArgs.Count == 0)
                {
                    success = false;
                    break;
                }

                args[i] = validArgs[Random.Shared.Next(validArgs.Count)];
            }

            if (!success)
                continue;

            _playerOperationSynchronizer.PushOperation(new PlayerSlowPrepareUnitSkill(
                Id,
                u.Coords,
                skillIndex,
                args,
                800));
        }
        await GDTask.Delay(800); 
        _playerOperationSynchronizer.PushOperation(new PlayerEndTurn(Id));
    }
}