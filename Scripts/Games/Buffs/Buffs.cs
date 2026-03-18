using System.Collections.Generic;

namespace TheBlockWar.Scripts.Games.Buffs;

public static class Buffs
{
    private static readonly Dictionary<BuffTypeEnum, IBuff> _buffs = new();
    public static void Register(IBuff buff)
    {
        _buffs[buff.Info.BuffType] = buff;
    }
    public static IBuff GetInstance(this BuffTypeEnum type)
    {
        return _buffs[type];
    }

    static Buffs()
    {
        Register(new TestBuff());
        Register(new Test2Buff());
    }
}