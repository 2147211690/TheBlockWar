using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass]
public partial class EffectTable : Resource
{
    [Export] public Godot.Collections.Dictionary<string, PackedScene> EffectScenes { get; set; } = new();
    public ViewNode2D? CreateEffect(IGameView view, string effectName)
    {
        if (!EffectScenes.TryGetValue(effectName, out var scene)) return null;
        var effect = scene.Instantiate<ViewNode2D>();
        view.DrawContainer.AddChild(effect);
        return effect;
    }
}