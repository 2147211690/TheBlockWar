namespace TheBlockWar.Scripts.Games.Buffs;

public class EmptyBuff : Buff
{
    public EmptyBuff(string name) : base(name)
    {
        Logger.Info($"EmptyBuff {name} 创建");
    }
}