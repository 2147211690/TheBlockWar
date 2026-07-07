namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class TurnStateData
{
    public int CurrentTurn { get; init; }
    public required Dictionary<int, Dictionary<string, int>> PhaseOrder { get; init; }
}
