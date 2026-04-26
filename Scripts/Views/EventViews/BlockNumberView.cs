using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class BlockNumberView : Node2D
{
    [Export] public EventsView EventsView { get; set; } = null!;
    private TilesView TilesView => EventsView.GameView.TilesView;
    private ViewPools ViewPools => EventsView.GameView.ViewPools;
    private Dictionary<Vector2I, Block> _blockNumberSprites = new();
    
    public void UseBlock(Vector2I coords)
    {
        if (_blockNumberSprites.TryGetValue(coords, out var block)) block.UseCount++;
        else
        {
            block = new Block
            {
                Sprite = ViewPools.Sprite2DPool.Pop(),
                ValidNumber = 0,
                HasAim = 0,
                UseCount = 1,
                IsFocused = 0,
            };
            block.Sprite.ZIndex = 2;
            _blockNumberSprites[coords] = block;
        }
    }

    public void CancelBlock(Vector2I coords)
    {
        if (!_blockNumberSprites.TryGetValue(coords, out var block)) return;
        block.UseCount--;
        if (block.UseCount <= 0)
        {
            RemoveBlock(coords);
        }
    }

    public void AddNumber(Vector2I coords, float rawValue, float validValue, bool hasAim, bool isFocused)
    {
        if (!_blockNumberSprites.TryGetValue(coords, out var block)) return;
        //被分配出的有效数值
        block.ValidNumber += hasAim ? validValue : 0;
        //未被分配出的数值
        block.RawNumber += rawValue;
        block.HasAim += hasAim ? 1 : 0;
        block.IsFocused += isFocused ? 1 : 0;
        
        UpdateBlock(block, coords);
    }

    public void RemoveNumber(Vector2I coords, float rawValue, float validValue, bool hasAim, bool isFocused)
    {
        if (!_blockNumberSprites.TryGetValue(coords, out var block)) return;
        //被分配出的有效数值
        block.ValidNumber = hasAim ? validValue : 0;
        //未被分配出的数值
        block.RawNumber -= rawValue;
        block.HasAim -= hasAim ? 1 : 0;
        block.IsFocused -= isFocused ? 1 : 0;
        
        UpdateBlock(block, coords);
    }
    private void UpdateBlock(Block block, in Vector2I coords)
    {
        var num = 0f;
        num = block.HasAim == 0 ? block.RawNumber : block.ValidNumber;
        var texture = EventsView.GameView.Style[0].GetBlockNumberTexture(Mathf.Abs((int)(num / 10)));
        block.Sprite.Texture = texture;
        block.Sprite.Modulate = num != 0
            ? (num > 0 ? Colors.GreenYellow : Colors.OrangeRed) :
            Colors.White;
        block.Sprite.Modulate = block.Sprite.Modulate with { A = block.IsFocused > 0 ? 1 : 0.6f };
        block.Sprite.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 1);
        block.Sprite.SetScaleCentered(
            TilesView.GetRenderOffset(coords, Layer.Sky, 1),
            block.HasAim > 0 ? new Vector2(1.2f, 1.2f) : Vector2.One);
    }

    private void RemoveBlock(Vector2I coords)
    {
        if (_blockNumberSprites.TryGetValue(coords, out var block))
        {
            block.Sprite.Texture = null;
            block.Sprite.Scale = Vector2.One;
            block.Sprite.Modulate = Colors.White;
            block.Sprite.ZIndex = 0;
            ViewPools.Sprite2DPool.Push(block.Sprite);
            _blockNumberSprites.Remove(coords);
        }
    }



    private class Block
    {
        public required Sprite2D Sprite { get; init; }
        public float ValidNumber { get; set; }
        public float RawNumber { get; set; }
        public int HasAim { get; set; }
        public int UseCount { get; set; }
        public int IsFocused { get; set; }
    }
}