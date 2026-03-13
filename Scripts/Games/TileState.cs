using System;

namespace TheBlockWar.Scripts.Games;

public class TileState : IReadOnlyTileState
{
    public static readonly TileState Empty = new() { Tag = TileTag.Empty };
    public TileTag Tag { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }

    public TileState Clone()
    {
        return new TileState
        {
            Tag = Tag,
            Name = Name,
            IsOccupied = IsOccupied,
        };
    }
}
[Flags]
public enum TileTag
{
    Empty = 1,
    Water = 1 << 1,
    Ground  = 1 << 2,
}