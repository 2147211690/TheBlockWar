using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Units;
using TheBlockWar.Scripts.Tools;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games;
//TODO:可能使用其他方式创建单位
public static class UnitFactory
{
    public static UnitState CreateUnit(string unitName, int team, long id, Vector2I coords = default)
    {
        var unitInfo = unitName.GetUnitInfo();
        return new UnitState()
        {
            Id = id,
            Name = unitName,
            Coords = coords,
            MaxHealth = unitInfo.MaxHealth,
            Health = unitInfo.MaxHealth,
            MaxShield = unitInfo.MaxShield,
            IsFlying = unitInfo.IsFlying,
            Shield = 0,
            Team = team,
        };
    }
}