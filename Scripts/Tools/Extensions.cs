using System.Linq;
using System.Text;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Views;

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

        public (Vector2I coords, bool isHit) CastRayToUnit(in Vector2I start, in Vector2I step, int maxSteps, bool checkGround = false)
        {
            var coords = start;
            for (var i = 0; i < maxSteps; i++)
            {
                coords += step;
                if (gameState.IsInMap(coords) && (!checkGround || !gameState.TileMapState.IsEmpty(coords)))
                {
                    if (gameState.UnitMapState.HasUnit(coords)) return (coords, true);
                }
                else
                {
                    return (coords, false);
                }
            }
            return (coords, false);
        }
    }

    extension(SimObject obj)
    {
        public (Vector2I coords, bool isHit) MoveHit(in Vector2I start, DirMask dir, float damage, int length, bool checkGround,
            SimContext context)
        {
            if (!dir.IsOneVec()) return (start, false);
            var (aim, isHit) = obj.State.CastRayToUnit(start, dir.ToVec2I(), length, checkGround);
            obj.ApplyMoveUnitPath(start, CoordsRegion.GetPathRegion(Vector2I.Zero, aim - dir.ToVec2I() - start).ToList());
            if (isHit)
            {
                obj.ApplyDamage(aim, damage);
                obj.ApplyDamage(aim - dir.ToVec2I(), damage);
            }
            return (aim, isHit);
        }
    }

    extension(Node2D node2d)
    {
        public void DrawTextureCentered(Texture2D? texture, Vector2 position, Color? modulate = null)
        {
            if (texture == null) return;
            node2d.DrawTexture(texture, position - (texture?.GetSize() ?? Vector2.One) * node2d.GlobalScale / 2,
                modulate);
        }

        /// <summary>
        /// 绘制纹理并根据偏移量调整位置
        /// </summary>
        /// <param name="offsetX">X轴偏移量，0-1之间的浮点数，0表示左边缘，1表示右边缘</param>
        /// <param name="offsetY">Y轴偏移量，0-1之间的浮点数，0表示上边缘，1表示下边缘</param>
        public void DrawTextureOffset(Texture2D? texture, Vector2 position, float offsetX, float offsetY,
            Color? modulate = null)
        {
            var size = texture?.GetSize() ?? Vector2.One;
            size = new(Mathf.Lerp(0, size.X, offsetX), Mathf.Lerp(0, size.Y, offsetY));
            node2d.DrawTexture(texture, position - size * node2d.GlobalScale, modulate);
        }

        public Rect2 DrawItemTab(Vector2 position, IReadOnlyList<Texture2D> textures, ItemTabStyle style)
        {
            if (textures.Count == 0)
            {
                // 返回一个空矩形 
                return new Rect2(position, Vector2.Zero);
            }

            // 计算整体宽度 
            float totalWidth = style.ItemSize.X * textures.Count + style.ItemSpacing * (textures.Count - 1);
            
            Vector2 startPos = position;
            startPos.Y -= style.ItemSize.Y / 2;

            // 根据对齐方式调整起始位置 
            if (style.Layout == 1) // 右对齐：以Position为右边，向左绘制 
            {
                startPos = new Vector2(startPos.X - totalWidth, startPos.Y);
            }
            else if (style.Layout == 0) // 居中对齐：所有元素在Position居中绘制 
            {
                startPos = new Vector2(startPos.X - totalWidth / 2, startPos.Y);
            }
            // style.Layout == -1 是左对齐，不需要调整，以Position为起点向右绘制 

            // 绘制每个纹理 
            Vector2 currentPos = startPos;
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i]; 
                node2d.DrawTextureRadioRect(texture, new Rect2(currentPos, style.ItemSize), style.Modulate);
                currentPos.X += style.ItemSize.X + style.ItemSpacing;
            }
            // 返回绘制区域的边界矩形 
            return new Rect2(startPos, totalWidth, style.ItemSize.Y);
        }
        public void DrawTextureRadioRect(Texture2D? texture, in Rect2 rect, Color? modulate = null)
        {
            if (texture == null) return;
            Vector2 textureSize = texture.GetSize();
            float texWidth = textureSize.X;
            float texHeight = textureSize.Y;

            float targetWidth = rect.Size.X;
            float targetHeight = rect.Size.Y;

            // 3. 计算等比缩放比例（取更小的，保证图片完全显示）
            float scaleX = targetWidth / texWidth;
            float scaleY = targetHeight / texHeight;
            float finalScale = Mathf.Min(scaleX, scaleY);

            // 4. 计算最终绘制的宽高（等比）
            float drawWidth = texWidth * finalScale;
            float drawHeight = texHeight * finalScale;

            // 5. 居中计算：在 rect 区域内水平、垂直居中
            float offsetX = rect.Position.X + (targetWidth - drawWidth) / 2f;
            float offsetY = rect.Position.Y + (targetHeight - drawHeight) / 2f;

            // 6. 最终绘制区域
            Rect2 drawRect = new Rect2(offsetX, offsetY, drawWidth, drawHeight);
            // 8. 执行绘制
            node2d.DrawTextureRect(texture, drawRect, false, modulate);
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

        public bool IsEndValidArgs(int index, out Vector2I result)
        {
            return args.TryGetArgs(index, out result) && !args.TryGetArgs(index + 1, out _);
        }
    }

    extension(in Vector2 pos)
    {
        public Vector3 ToVec3(float z = 0) => new(pos.X, pos.Y, z);
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
                _ => DirMask.None
            } | c.Y switch
            {
                0 => DirMask.None,
                1 => DirMask.Down,
                -1 => DirMask.Up,
                _ => DirMask.None
            };
        }
        public Vector2 ToVec2() => new(coords.X, coords.Y);
        public Vector3 ToVec3(float z = 0) => new(coords.X, coords.Y, z);
        public Vector3 ToUnderVec3(IGameView gameView) => new(coords.X, coords.Y, gameView.GetTileHeight(coords));
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
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(EventScheduler eventScheduler)
        {
            return eventScheduler.GameState?.TeamState.GetTeamTypeRelativeToPlayer(eventScheduler.PlayerTeam, unit.Team) ?? TeamTypeEnum.Enemy;
        }
        public bool TryFindBuff(string buffName, out int index)
        {
            for (var i = 0; i < unit.Buffs.Count; i++)
                if (unit.Buffs[i].BuffName == buffName)
                {
                    index = i;
                    return true;
                }
            index = -1;
            return false;
        }
        public int GetBuffCount(string buffName)
        {
            if (!unit.TryFindBuff(buffName, out var index)) return 0;
            return unit.Buffs[index].Count;
        }
        public int GetMovePower(IReadOnlyGameState state)
        {
            var defaultValue = unit.Name.GetUnitInfo().MovePower;
            var value = defaultValue;
            Hook.BuffUnitGetMovePower(unit, state, defaultValue, ref value);
            return value;
        }
        public bool GetShouldUseSkill(IReadOnlyGameState state, ISkill skill)
        {
            if (unit.IsDead) return false;
            var shouldUseSkill = true;
            Hook.BuffUnitShouldUseSkill(unit, state, skill, ref shouldUseSkill);
            return shouldUseSkill;
        }
        public bool IsNull() => unit.Id == 0;
    }

    extension(IReadOnlyTileState tile)
    {
        public bool TryFindRegion(string regionName, out int index)
        {
            for (var i = 0; i < tile.Regions.Count; i++)
                if (tile.Regions[i].RegionName == regionName)
                {
                    index = i;
                    return true;
                }
            index = -1;
            return false;
        }
        public int GetRegionDuration(string regionName)
        {
            if (!tile.TryFindRegion(regionName, out var index)) return 0;
            return tile.Regions[index].Duration;
        }
        public bool IsNull() => tile.Name == "Air" || tile.Name.IsWhiteSpace();
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
    extension<T>(IReadOnlyList<T>? list)
    {
        public List<T>? CloneToListOrNullWhenCountZero()
        {
            if (list is not { Count: > 0 }) return null;
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
        public Vector2 ToVec2()
        {
            return dirMask switch
            {
                DirMask.Up => new Vector2(0, -1),
                DirMask.Down => new Vector2(0, 1),
                DirMask.Left => new Vector2(-1, 0),
                DirMask.Right => new Vector2(1, 0),
                _ => Vector2.Zero,
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

        public bool IsOneVec()
        {
            return dirMask != DirMask.None && (dirMask & (dirMask - 1)) == 0;
        }
    }

    extension(Tween tween)
    {
        public GDTask WaitForTween()
        {
            return tween.ToSignal(tween, Tween.SignalName.Finished).AsGDTask();
        }
        public GDTask WaitForTween(Action<Tween> action)
        {
            var complete = new GDTaskCompletionSource();
            tween.Finished += () =>
            {
                action.Invoke(tween);
                complete.TrySetResult();
            };
            return complete.Task;
        }
    }
    extension(BuffOperation operation)
    {
        public uint WithDuration(int duration)
        {
            return (uint)operation | (uint)duration;
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
        public TeamTypeEnum GetTeamTypeRelativeToPlayer(EventScheduler eventScheduler)
        {
            return eventScheduler.GameState?.TeamState.GetTeamTypeRelativeToPlayer(eventScheduler.PlayerTeam, team) ?? TeamTypeEnum.Enemy;
        }
    }
    extension(AnimatedSprite2D sprite2D)
    {
        public bool TryPlay(StringName animation)
        {
            if (!(sprite2D.SpriteFrames?.HasAnimation(animation) ?? false)) return false;
            sprite2D.Play(animation);
            return true;
        }
    }

    extension(EventScheduler eventScheduler)
    {
        public void RunAction(Action<SimObject> action)
        {
            eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (_, o) => action(o),
                Layer = SimulationLayer.Running,
            });
        }
    }
}

public class ItemTabStyle
{
    public Vector2 ItemSize { get; set; }
    public float ItemSpacing { get; set; } = 10;
    /// <summary>
    /// 0: 水平布局
    /// -1: 左对齐
    /// 1: 右对齐
    /// </summary>
    public int Layout { get; set; } = 0;
    public Color Modulate { get; set; } = Colors.White;
}