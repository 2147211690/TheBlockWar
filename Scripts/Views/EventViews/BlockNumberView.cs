using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class BlockNumberView : Node2D
{
    [Export] public EventsView EventsView { get; set; }
    private TilesView TilesView => EventsView.GameView.TilesView;
    private ViewPools ViewPools => EventsView.GameView.ViewPools;
    private Dictionary<Vector2I, Block> _blockNumberSprites = new();
    
    public void UseBlock(Vector2I coords)
    {
        if (_blockNumberSprites.TryGetValue(coords, out var block)) block.IsFocused++;
        else
        {
            block = new Block
            {
                Sprite = ViewPools.Sprite2DPool.Pop(),
                FocusedNumber = 0,
                HasAim = 0,
                IsFocused = 1,
            };
            _blockNumberSprites[coords] = block;
        }
    }

    public void CancelBlock(Vector2I coords)
    {
        if (!_blockNumberSprites.TryGetValue(coords, out var block)) return;
        block.IsFocused--;
        if (block.IsFocused <= 0)
        {
            RemoveBlock(coords);
        }
    }

    public void AddNumber(Vector2I coords, float value, bool hasAim, bool isFocused)
    {
        if (!_blockNumberSprites.TryGetValue(coords, out var block)) return;
        block.FocusedNumber += isFocused ? value : 0;
        block.Number += !isFocused ? value : 0;
        block.HasAim += hasAim ? 1 : 0;
        
        UpdateBlock(block, coords);
    }
    private void UpdateBlock(Block block, in Vector2I coords)
    {
        var num = block.IsFocused > 0 ? block.FocusedNumber : block.Number;
        var texture = EventsView.GameView.Style[0].GetBlockNumberTexture(Mathf.Abs((int)(num / 10)));
        block.Sprite.Texture = texture;
        block.Sprite.Scale = block.HasAim > 0 ? new Vector2(1.2f, 1.2f) : Vector2.One;
        block.Sprite.Modulate = block.HasAim > 0
            ? (block.FocusedNumber > 0 ? Colors.GreenYellow : Colors.OrangeRed)
            : Colors.White;
        block.Sprite.Modulate = block.Sprite.Modulate with { A = block.IsFocused > 0 ? 1 : 0.6f };
        block.Sprite.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0);
        block.Sprite.Offset = TilesView.GetRenderOffset(coords, Layer.Sky, 0, 20);
    }

    private void RemoveBlock(Vector2I coords)
    {
        if (_blockNumberSprites.TryGetValue(coords, out var block))
        {
            block.Sprite.Texture = null;
            block.Sprite.Scale = Vector2.One;
            block.Sprite.Modulate = Colors.White;
            ViewPools.Sprite2DPool.Push(block.Sprite);
            _blockNumberSprites.Remove(coords);
        }
    }



    private class Block
    {
        public required Sprite2D Sprite { get; init; }
        public float FocusedNumber { get; set; }
        public float Number { get; set; }
        public int HasAim { get; set; }
        public int IsFocused { get; set; }
    }
}