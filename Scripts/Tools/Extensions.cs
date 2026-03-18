using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public static class Extensions
{
    extension(IReadOnlyGameState gameState)
    {
        public string ToInfoString()
        {
            var units = gameState.UnitMapState.GetAllUnits().ToList();
            var mapSize = gameState.TileMapState.Size;
            
            var sb = new StringBuilder();
            sb.AppendLine($"GameState - Map Size: {mapSize.X}x{mapSize.Y}");
            sb.AppendLine($"Units Count: {units.Count}");
            
            if (units.Count > 0)
            {
                sb.AppendLine("Units:");
                foreach (var unit in units)
                {
                    sb.AppendLine(
                        $"  Unit {unit.Id} at ({unit.Coords.X}, {unit.Coords.Y}) - Health: {unit.Health}/{unit.MaxHealth}, Shield: {unit.Shield}/{unit.MaxShield}, IsDead: {unit.IsDead}");
                }
            }
            
            return sb.ToString();
        }
        public bool IsInMap(in Vector2I coords)
        {
            var size = gameState.TileMapState.Size;
            return coords.IsInSize(size);
        }
    }
    
    extension(Node2D node2d)
    {
        public void DrawTextureCentered(Texture2D texture, Vector2 position, Color? modulate = null)
        {
            node2d.DrawTexture(texture, position - texture.GetSize() * node2d.GlobalScale / 2, modulate);
        }
        /// <summary>
        /// 绘制纹理并根据偏移量调整位置
        /// </summary>
        /// <param name="offsetX">X轴偏移量，0-1之间的浮点数，0表示左边缘，1表示右边缘</param>
        /// <param name="offsetY">Y轴偏移量，0-1之间的浮点数，0表示上边缘，1表示下边缘</param>
        public void DrawTextureOffset(Texture2D texture, Vector2 position, float offsetX, float offsetY, Color? modulate = null)
        {
            var size = texture.GetSize();
            size = new(Mathf.Lerp(0, size.X, offsetX), Mathf.Lerp(0, size.Y, offsetY));
            node2d.DrawTexture(texture, position - size * node2d.GlobalScale, modulate);
        }
    }

    extension(Vector2I[] args)
    {
        public bool TryGetArgs(int index, out Vector2I result)
        {
            if (index < 0 || index >= args.Length)
            {
                result = default;
                return false;
            }
            result = args[index];
            return true;
        }
    }
    extension(Vector2I?[] args)
    {
        public bool TryGetArgs(int index, out Vector2I result)
        {
            if (index < 0 || index >= args.Length || args[index] is null)
            {
                result = default;
                return false;
            }
            result = args[index]!.Value;
            return true;
        }
    }

    extension(in Vector2I coords)
    {
        public DirMask ToVecDirMask()
        {
            var c = coords with { X = Mathf.Sign(coords.X), Y = Mathf.Sign(coords.Y) };
            return c.X switch
            {
                0 => DirMask.None,
                1 => DirMask.Right,
                -1 => DirMask.Left,
            } | c.Y switch
            {
                0 => DirMask.None,
                1 => DirMask.Down,
                -1 => DirMask.Up,
            };
        }
        public bool IsInSize(in Vector2I size, Vector2I offset = default)
        {
            return coords.X >= 0 + offset.X && coords.X < size.X + offset.X && coords.Y >= 0 + offset.Y && coords.Y < size.Y + offset.Y;
        }
    }

    extension(IReadOnlyUnitState unit)
    {
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(IReadOnlyGameState gameState, int playerTeam)
        {
            return gameState.TeamState.GetTeamTypeRelativeToPlayer(playerTeam, unit.Team);
        }
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(GameScheduler gameScheduler)
        {
            return gameScheduler.GameState?.TeamState.GetTeamTypeRelativeToPlayer(gameScheduler.PlayerTeam, unit.Team) ?? TeamTypeEnum.Neutral;
        }
        public bool TryFindBuff(BuffTypeEnum buffType, out int index)
        {
            for (var i = 0; i < unit.Buffs.Count; i++)
                if (unit.Buffs[i].BuffType == buffType)
                {
                    index = i;
                    return true;
                }
            index = -1;
            return false;
        }
        public bool IsNull() => unit.Id == 0;
    }

    extension(IReadOnlyTileState tile)
    {
        public bool TryFindRegion(RegionTypeEnum regionType, out int index)
        {
            for (var i = 0; i < tile.Regions.Count; i++)
                if (tile.Regions[i].RegionType == regionType)
                {
                    index = i;
                    return true;
                }
            index = -1;
            return false;
        }
        public bool IsNull() => (tile.Tag & TileTag.Empty) != 0;
    }
    

    extension<T>(IReadOnlyList<T> list)
    {
        public List<T> CloneToList()
        {
            var l = new List<T>(list.Count);
            for (var i = 0; i < list.Count; i++) l.Add(list[i]);
            return l;
        }
    }
    extension(DirMask dirMask)
    {
        public Vector2I ToVec2I()
        {
            return dirMask switch
            {
                DirMask.Up => new Vector2I(0, -1),
                DirMask.Down => new Vector2I(0, 1),
                DirMask.Left => new Vector2I(-1, 0),
                DirMask.Right => new Vector2I(1, 0),
                _ => Vector2I.Zero,
            };
        }
        public int ToIndex()
        {
            return dirMask switch
            {
                DirMask.Up => 0,
                DirMask.Down => 1,
                DirMask.Left => 2,
                DirMask.Right => 3,
                _ => -1,
            };
        }
    }

    extension(Tween tween)
    {
        public GDTask WaitForTween()
        {
            return tween.ToSignal(tween, Tween.SignalName.Finished).AsGDTask();
        }
    }
    extension(BuffOperation operation)
    {
        public uint WithDuration(int duration)
        {
            return (uint)operation | (uint)duration;
        }
    }

    extension(uint num)
    {
        public BuffOperation GetOperation()
        {
            return (BuffOperation)(num & (0b_111 << 29));
        }
        public int GetDuration()
        {
            return (int)(num & ~(0b_011 << 29));
        }
    }

    extension(AnimatedSprite2D s)
    {
        public void ContinueFinishedOneShot(Action action)
        {
            s.Connect(AnimatedSprite2D.SignalName.AnimationFinished,
                Callable.From(action), (uint)GodotObject.ConnectFlags.OneShot);
        }
        /// <summary>
        /// 在以当前偏移量为中心的基础上，设置缩放比例, 并更新偏移量。
        /// </summary>
        public void SetScaleCentered(in Vector2 offset, in Vector2 scale)
        {
            s.Offset = s.Scale * offset / scale;
            s.Scale = scale;
        }
    }
    extension(Sprite2D s)
    {
        /// <summary>
        /// 在以当前偏移量为中心的基础上，设置缩放比例, 并更新偏移量。
        /// </summary>
        public void SetScaleCentered(in Vector2 offset, in Vector2 scale)
        {
            s.Offset = offset / scale;
            s.Scale = scale;
        }
    }

    extension(int team)
    {
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(IReadOnlyGameState gameState, int playerTeam)
        {
            return gameState.TeamState.GetTeamTypeRelativeToPlayer(playerTeam, team);
        }
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(GameScheduler gameScheduler)
        {
            return gameScheduler.GameState?.TeamState.GetTeamTypeRelativeToPlayer(gameScheduler.PlayerTeam, team) ?? TeamTypeEnum.Neutral;
        }
    }
    
}