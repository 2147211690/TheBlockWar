using System.ComponentModel;
using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;

public class EasyAiPlayer : IPlayer
{
    public EasyAiPlayer(int team)
    {
        Team = team;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public EventScheduler? GameScheduler { get; set; }
    public IReadOnlyList<SkillState> SkillingUnitSkillStates { get; }
    public long SelectedUnitId { get; }
    public int Power { get; }
    public int Team { get; }

    public bool IsTurn
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                TurnAsync().Forget();
            }
        }
    }

    public int PhaseOrder { get; set; }
    public void OnInput<T>(in T input)
    {
    }

    public void Init()
    {
    }

    public GDTask WaitTurnComplete()
    {
        return GDTask.WaitUntil(() => !IsTurn);
    }

    public async GDTask TurnAsync()
    {
        var state = GameScheduler!.GameState;
        var units = state.UnitMapState.GetAllUnits().Where(u => u.Team == Team && !u.IsDead).ToList();
        var nextTurn = GameScheduler.CurrentTurn + 1;

        foreach (var u in units)
        {
            if (GameScheduler.GetUnitSkillEvent(u.Id) is not null)
                continue;

            var skills = state.GetUnitValidSkills(u.Name);
            var validSkills = skills.Where(s => u.GetShouldUseSkill(state, s)).ToList();
            if (validSkills.Count == 0)
                continue;

            var skill = validSkills[Random.Shared.Next(validSkills.Count)];
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

            var e = GameScheduler.CreateEvent(id => new SkillEvent(id)
            {
                SkillBindUnitId = u.Id,
                SkillName = skill.SkillName,
                Team = u.Team,
                SkillArgs = args,
                Turn = nextTurn,
                Layer = SimulationLayer.Focused,
            });
            await GDTask.Delay(800);
            e.Layer = SimulationLayer.Waiting;
        }
        await GDTask.Delay(800);
        IsTurn = false;
    }
}