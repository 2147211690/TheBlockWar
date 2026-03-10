using System;

namespace TheBlockWar.Scripts.Games;

public struct TileState
{
    public static readonly TileState Empty = new() { Tag = TileTag.Empty };
    public TileTag Tag { get; set; }
    public string Id { get; set; }
    public bool IsOccupied { get; set; }

    public TileState Clone()
    {
        return new TileState
        {
            Tag = Tag,
            Id = Id,
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