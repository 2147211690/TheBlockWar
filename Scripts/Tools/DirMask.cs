using System;

namespace TheBlockWar.Scripts.Tools;

[Flags]
public enum DirMask
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
    All = Up | Down | Left | Right,
    UpDown = Up | Down,
    UpLeft = Up | Left,
    UpRight = Up | Right,
    DownLeft = Down | Left,
    DownRight = Down | Right,
    LeftRight = Left | Right,
}