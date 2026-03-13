namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyTileState
{
    TileTag Tag { get; }
    string Name { get; }
    bool IsOccupied { get; }
    TileState Clone();
}