using System.ComponentModel;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Players;

public interface IPlayer : INotifyPropertyChanged
{
    IEventScheduler? GameScheduler { get; set; }
    long SelectedUnitId { get; }
    int Power { get; }
    int Team { get; set; }
    bool IsTurn { get; set; }
    TurnManager TurnManager { get; set; }
    int PhaseOrder { get; set; }
    int CurrentTurn { get; set; }
    bool WaitConfirm { get; set; }
    void OnInput<T>(in T input);
    long Id { get; }
    void Init();
}
