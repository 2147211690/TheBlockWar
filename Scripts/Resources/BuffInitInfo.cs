namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class BuffInitInfo : Resource
{
    [Export] public string BuffName { get; set; } = "None";
    [Export] public int Count { get; set; } = 0;
}