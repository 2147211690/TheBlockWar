using System.ComponentModel;

namespace TheBlockWar.Scripts.Games.Players;

public class PassPlayer : IPlayer
{
    public PassPlayer(int team)
    {
        Team = team;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public EventScheduler? GameScheduler { get; set; }
    public IReadOnlyList<SkillState> SkillingUnitSkillStates { get; }
    public long SelectedUnitId { get; }
    public int Power { get; }
    public int Team { get; }
    public bool IsTurn { get => false; set; }
    public int PhaseOrder { get; set; }
    public void OnInput<T>(in T input)
    {
    }

    public void Init()
    {
    }
}