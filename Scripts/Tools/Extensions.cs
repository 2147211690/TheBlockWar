using System.Linq;
using System.Text;
using Godot;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Tools;

public static class Extensions
{
    extension(IGameState gameState)
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
            if (index < 0 || index >= args.Length || args[index] == null)
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
    }
}