using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Tools;
using Wintellect.PowerCollections;

namespace TheBlockWar.Scripts.Views.EventViews;

[GlobalClass]
public partial class BlockBuffView : Node2D
{
    [Export] public EventsView EventsView { get; set; } = null!;
    [Export] public Vector2 IconSize { get; set; } = new Vector2(32, 32);
    [Export] public Vector2 SymbolBoxSize { get; set; } = new Vector2(24, 24);
    [Export] public float ElementSpacing { get; set; } = 2f;
    [Export] public float RowSpacing { get; set; } = 4f;

    [ExportGroup("符号纹理")]
    [Export] public Texture2D? PlusTexture { get; set; }
    [Export] public Texture2D? MinusTexture { get; set; }

    [ExportGroup("颜色设置")]
    [Export] public Color AddTint { get; set; } = Colors.White;
    [Export] public Color RemoveTint { get; set; } = Colors.OrangeRed;

    private TilesView TilesView => EventsView.GameView.TilesView;

    private MultiDictionary<Vector2I, BuffNameEnum> _coordsForAddBuff = new(false);
    private MultiDictionary<Vector2I, BuffNameEnum> _coordsForRemoveBuff = new(false);
    private MultiDictionary<Vector2I, BuffNameEnum> _coordsForAddBuffFocused = new(false);
    private MultiDictionary<Vector2I, BuffNameEnum> _coordsForRemoveBuffFocused = new(false);
    private Bag<(Vector2I, BuffNameEnum)> _coordsForAddBuffCount = new();
    private Bag<(Vector2I, BuffNameEnum)> _coordsForRemoveBuffCount = new();
    private Bag<(Vector2I, BuffNameEnum)> _coordsForAddBuffFocusedCount = new();
    private Bag<(Vector2I, BuffNameEnum)> _coordsForRemoveBuffFocusedCount = new();
    
    

    private double _animationTime;
    
    public void AddBuffChange(Vector2I coords, BuffNameEnum buffName, bool isAdd, bool isFocused = false)
    {
        var (countDict, multiDict) = GetDictPair(isAdd, isFocused);
        
        multiDict.Add(coords, buffName);
        countDict.Add((coords, buffName));
        QueueRedraw();
    }

    public void RemoveBuffChange(Vector2I coords, BuffNameEnum buffName, bool isAdd, bool isFocused = false)
    {
        var (countDict, multiDict) = GetDictPair(isAdd, isFocused);
        
        if (countDict.Remove((coords, buffName)) && !countDict.Contains((coords, buffName)))
        {
            multiDict.Remove(coords, buffName); 
        }
        QueueRedraw();
    }

    private (Bag<(Vector2I, BuffNameEnum)> countDict, MultiDictionary<Vector2I, BuffNameEnum> multiDict) GetDictPair(bool isAdd, bool isFocused)
    {
        Bag<(Vector2I, BuffNameEnum)> countDict;
        MultiDictionary<Vector2I, BuffNameEnum> multiDict;
        if (isFocused)
        {
            countDict = isAdd ? _coordsForAddBuffFocusedCount : _coordsForRemoveBuffFocusedCount;
            multiDict = isAdd ? _coordsForAddBuffFocused : _coordsForRemoveBuffFocused;
        }
        else
        {
            countDict = isAdd ? _coordsForAddBuffCount : _coordsForRemoveBuffCount;
            multiDict = isAdd ? _coordsForAddBuff : _coordsForRemoveBuff;
        }

        return (countDict, multiDict);
    }
    
    private bool IsFocused(Vector2I coords)
    {
        return _coordsForAddBuffFocused.ContainsKey(coords) || _coordsForRemoveBuffFocused.ContainsKey(coords);
    }

    public override void _Draw()
    {
        base._Draw();

        var allCoords = new HashSet<Vector2I>();
        foreach (var kvp in _coordsForAddBuff) allCoords.Add(kvp.Key);
        foreach (var kvp in _coordsForRemoveBuff) allCoords.Add(kvp.Key);
        foreach (var kvp in _coordsForAddBuffFocused) allCoords.Add(kvp.Key);
        foreach (var kvp in _coordsForRemoveBuffFocused) allCoords.Add(kvp.Key);

        if (allCoords.Count == 0) return;

        foreach (var coords in allCoords)
        {
            DrawBuffAtCoords(coords);
        }
    }

    private void DrawBuffAtCoords(Vector2I coords)
    {
        Vector2 worldPos = TilesView.GetRealPosition(coords, Layer.Underground);

        bool isFocused = IsFocused(coords);

        var addBuffs = isFocused ? _coordsForAddBuffFocused[coords] : _coordsForAddBuff[coords];
        var removeBuffs = isFocused ? _coordsForRemoveBuffFocused[coords] : _coordsForRemoveBuff[coords];

        int rowCount = (addBuffs.Count > 0 ? 1 : 0) + (removeBuffs.Count > 0 ? 1 : 0);
        if (rowCount == 0) return;
        
        float singleRowHeight = Mathf.Max(SymbolBoxSize.Y, IconSize.Y);
        float totalHeight = rowCount * singleRowHeight + (rowCount > 1 ? RowSpacing : 0);
        float currentRowCenterY = -totalHeight / 2 + singleRowHeight / 2;

        if (addBuffs.Count > 0)
        {
            DrawBuffRow(worldPos + new Vector2(0, currentRowCenterY), addBuffs, true, isFocused ? 1 : 0.6f);
            currentRowCenterY += singleRowHeight + RowSpacing;
        }
        if (removeBuffs.Count > 0)
        {
            DrawBuffRow(worldPos + new Vector2(0, currentRowCenterY), removeBuffs, false, isFocused ? 1 : 0.6f);
        }
    }

    private void DrawBuffRow(Vector2 rowCenter, ICollection<BuffNameEnum> buffs, bool isGain, float alpha)
    {
        float totalWidth = SymbolBoxSize.X;
        if (buffs.Count > 0)
        {
            totalWidth += ElementSpacing + buffs.Count * IconSize.X;
            totalWidth += (buffs.Count - 1) * ElementSpacing;
        }

        float currentX = rowCenter.X - totalWidth / 2;
        float rowCenterY = rowCenter.Y;
        Color currentTint = isGain ? AddTint : RemoveTint;
        Texture2D? symbolTex = isGain ? PlusTexture : MinusTexture;

        if (symbolTex != null)
        {
            DrawTextureInBox(
                symbolTex,
                new Vector2(currentX, rowCenterY - SymbolBoxSize.Y / 2),
                SymbolBoxSize,
                currentTint,
                alpha
            );
        }
        currentX += SymbolBoxSize.X + ElementSpacing;

        foreach (var type in buffs)
        {
            var icon = type.GetInfo().Icon;
            if (icon != null)
            {
                DrawTextureInBox(
                    icon,
                    new Vector2(currentX, rowCenterY - IconSize.Y / 2),
                    IconSize,
                    currentTint,
                    alpha
                );
            }
            currentX += IconSize.X + ElementSpacing;
        }
    }

    private void DrawTextureInBox(Texture2D tex, Vector2 boxTopLeft, Vector2 boxSize, Color tint, float alpha)
    {
        float scale = Mathf.Min(boxSize.X / tex.GetWidth(), boxSize.Y / tex.GetHeight());
        Vector2 fitSize = new(tex.GetWidth() * scale, tex.GetHeight() * scale);
        Vector2 offset = (boxSize - fitSize) / 2;
        Vector2 drawPos = boxTopLeft + offset;

        Color finalColor = new Color(tint.R, tint.G, tint.B, alpha);
        DrawTextureRect(tex, new Rect2(drawPos, fitSize), false, finalColor);
    }

    public void Clear()
    {
        _coordsForAddBuff.Clear();
        _coordsForRemoveBuff.Clear();
        _coordsForAddBuffFocused.Clear();
        _coordsForRemoveBuffFocused.Clear();
        _coordsForAddBuffCount.Clear();
        _coordsForRemoveBuffCount.Clear();
        _coordsForAddBuffFocusedCount.Clear();
        _coordsForRemoveBuffFocusedCount.Clear();
    }
}