using System.Linq;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.EventViews;

[GlobalClass]
public partial class BlockEventDrawNode : ViewNode2D
{
    public IGameView GameView => Container.GameView;
    public Vector2I Coords { get; set; }
    
    
    private float _healthOriginChange;
    private float _healthValidChange;
    private bool _healthHasAim;

    private HashSet<string> _addBuffs = new();
    private HashSet<string> _removeBuffs = new();

    private HashSet<string> _addRegions = new();
    private HashSet<string> _removeRegions = new();
    
    
    private Texture2D[][] _tabs = new Texture2D[4][];
    private bool[] _addTab = new bool[4];
    private ItemTabStyle _itemTabStyle = new()
    {
        ItemSize = new(12,12),
        ItemSpacing = 0,
        Layout = 0,
        Modulate = Colors.White
    };
    private static readonly Vector2[] TabSizes = [new(15,15), new(13,13), new(10,10), new(8,8)];
    
    public TeamTypeEnum FocusedTeam { get; set; }
    public bool IsFocused { get; private set; }

    public void AddHealthChange(float originChange, float validChange, bool hasAim, bool isFocused, TeamTypeEnum team)
    {
        if (!UpdateFocused(isFocused, team)) return;
        _healthHasAim |= hasAim;
        _healthOriginChange += originChange;
        _healthValidChange += validChange;
        QueueRedraw();
    }

    public void AddBuffChange(string buffName, bool isAdd, bool isFocused, TeamTypeEnum team)
    {
        if (!UpdateFocused(isFocused, team)) return;
        if (isAdd)
        {
            _addBuffs.Add(buffName);
        }
        else
        {
            _removeBuffs.Add(buffName);
        }
        QueueRedraw();
    }

    public void AddRegionChange(string regionName, bool isAdd, bool isFocused, TeamTypeEnum team)
    {
        if (!UpdateFocused(isFocused, team)) return;
        if (isAdd)
        {
            _addRegions.Add(regionName);
        }
        else
        {
            _removeRegions.Add(regionName);
        }
        QueueRedraw();
    }

    public void Clear()
    {
        _healthOriginChange = 0;
        _healthValidChange = 0;
        _healthHasAim = false;
        _addBuffs.Clear();
        _removeBuffs.Clear();
        _addRegions.Clear();
        _removeRegions.Clear();
        IsFocused = false;
        QueueRedraw();
    }
    
    public override void _Draw()
    {

        var hasHelath = _healthOriginChange != 0;
        var tabCount = Mathf.Sign(_addBuffs.Count) + Mathf.Sign(_removeBuffs.Count) + Mathf.Sign(_addRegions.Count) + Mathf.Sign(_removeRegions.Count);
        var hasTab = tabCount > 0;
        if (!hasHelath && !hasTab) return;
        var healthPos = Vector2.Zero;
        var tabPos = Vector2.Zero;
        if (hasHelath == hasTab)
        {
            healthPos.X -= 5;
            tabPos.X += 5;
        }
        

        if (hasHelath)
        {
            var healthValue = IsFocused && _healthHasAim ? _healthValidChange : _healthOriginChange;
            var healtColor = healthValue > 0 ? Colors.Green : Colors.Red;
            healtColor.A = IsFocused ? 1 : 0.5f;
            this.DrawTextureCentered(
                GameView.CurrentStyle.GetBlockNumberTexture(Mathf.Abs((int)(healthValue / 10))),
                healthPos,
                healtColor);
        }

        if (hasTab)
        {
            var i = 0;
            if (_addBuffs.Count > 0)
            {
                _tabs[i] = _addBuffs.Select(a => a.GetBuffInfo().Icon).ToArray()!;
                _addTab[i++] = true;
            }
            if (_removeBuffs.Count > 0)
            {
                _tabs[i] = _removeBuffs.Select(a => a.GetBuffInfo().Icon).ToArray()!;
                _addTab[i++] = false;
            }
            if (_addRegions.Count > 0)
            {
                _tabs[i] = _addRegions.Select(a => a.GetBuffInfo().Icon).ToArray()!;
                _addTab[i++] = true;
            }
            if (_removeRegions.Count > 0)
            {
                _tabs[i] = _removeRegions.Select(a => a.GetBuffInfo().Icon).ToArray()!;
                _addTab[i++] = false;
            }
            _itemTabStyle.Layout = hasHelath == hasTab ? -1 : 0;
            _itemTabStyle.ItemSize = TabSizes[i];
            _itemTabStyle.Modulate = _itemTabStyle.Modulate with { A = IsFocused ? 1 : 0.5f };
            var spacing = TabSizes[i].Y + 1;
            
            var startPos = tabPos + new Vector2(0, -spacing * (tabCount - 1) / 2);
            for (var j = 0; j < tabCount; j++)
            {
                var pos = startPos + new Vector2(0, j * spacing);
                var rect = this.DrawItemTab(pos, _tabs[j], _itemTabStyle);
                if (!_addTab[j])
                {
                    var linePos = rect.Position + new Vector2(0, rect.Size.Y / 2);
                    DrawLine(linePos, linePos + new Vector2(rect.Size.X, 0), Colors.Red, 1);
                }
            }
        }
    }

    
    private void AddFocused(bool isFocused, TeamTypeEnum team)
    {
        if (!isFocused || IsFocused) return;
        IsFocused = true;
        _healthOriginChange = 0;
        FocusedTeam = team;
        _addBuffs.Clear();
        _removeBuffs.Clear();
        _addRegions.Clear();
        _removeRegions.Clear();
        QueueRedraw();
    }

    private bool CheckFocused(bool isFocused)
    {
        return !IsFocused || isFocused;
    }
    
    private bool UpdateFocused(bool isFocused, TeamTypeEnum team)
    {
        AddFocused(isFocused, team);
        return CheckFocused(isFocused);
    }
    
}