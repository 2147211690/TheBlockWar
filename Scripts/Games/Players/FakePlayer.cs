using System.ComponentModel;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Players;

public class FakePlayer : IPlayer
{
    public FakePlayer(int team, long id)
    {
        Team = team;
        Id = id;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public IEventScheduler? GameScheduler { get; set; }
    public long SelectedUnitId { get; }
    public int Power { get; }
    public int Team { get; set; }
    public bool IsTurn { get; set; }
    public PlayerTurnEnterEnum TurnEnter { get; set; }
    public ITurnManager TurnManager { get; set; }
    public int PhaseOrder { get; set; }
    public int CurrentTurn { get; set; }
    public int SelectedSkillIndex { get; set; }
    public Vector2I? SelectedTileCoords { get; set; }
    public bool WaitConfirm { get; set; }
    public void OnInput<T>(in T input)
    {
    }

    public long Id { get; }
    public void Init()
    {
    }
}