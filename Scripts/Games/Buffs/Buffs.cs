namespace TheBlockWar.Scripts.Games.Buffs;

public static class Buffs
{
    private static readonly Dictionary<BuffNameEnum, IBuff> _buffs = new();
    public static void Register(IBuff buff)
    {
        if (_buffs.TryAdd(buff.Name, buff)) return;
        GD.PushWarning($"{buff.GetType()}:另一个buff已注册为{buff.Name}");
    }
    public static IBuff GetInstance(this BuffNameEnum name)
    {
        if (_buffs.TryGetValue(name, out var buff)) return buff;
        GD.PushWarning($"未注册buff:{name},使用空buff");
        _buffs[name] = buff = new EmptyBuff(name);
        return buff;
    }
}
public class EmptyBuff(BuffNameEnum name) : Buff(name){}