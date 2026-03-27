using System.ComponentModel;

namespace TheBlockWar.Scripts.Games.Players;

public interface IPlayer : INotifyPropertyChanged
{
    GameScheduler? GameScheduler { get; set; }
    IReadOnlyList<SkillState> SkillingUnitSkillStates { get; }
    long SelectedUnitId { get; }
    int Power { get; }
    bool IsTurn { get; set; }
    void OnInput<T>(in T input);
    void Init();
}

public struct SkillState
{
    public ISkill Skill { get; set; }
    public int RemainingUses { get; set; }
    public bool IsSelecting { get; set; }
}