using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

public static class Regions
{
    private static readonly Dictionary<RegionTypeEnum, IRegion> _regions = new();
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public static void Register(IRegion region)
    {
        _regions[region.RegionType] = region;
    }
    public static IRegion GetInstance(this RegionTypeEnum type)
    {
        return _regions[type];
    }

    static Regions()
    {
        Register(new Test1());
        Register(new Test2());
    }
    
    class Region : IRegion
    {
        public Region(RegionTypeEnum regionType, bool isOnce, int duration)
        {
            RegionType = regionType;
            IsOnce = isOnce;
            Duration = duration;
        }
        public virtual RegionTypeEnum RegionType { get; }
        public bool IsOnce { get; }
        public int Duration { get; }

        public virtual void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationResult result)
        {
        }

        public virtual void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationResult result)
        {
        }

    }
    
    class Test1() : Region(RegionTypeEnum.Test1, true, 10)
    {
        public override void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, 
            bool isAddRegion, SimulationResult result)
        {
            Logger.Info($"Test1 OnUnitEnter {unit} {enterCoords}");
            state.ApplyPushUnit(enterCoords, DirMask.Right, result);
        }

        public override void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, 
            bool isRemoveRegion, SimulationResult result)
        {
            Logger.Info($"Test1 OnUnitLeave {unit} {leaveCoords}");
            state.ApplyDamage(unit.Coords, 30, result);
        }
    }
    
    class Test2() : Region(RegionTypeEnum.Test2, false, 10)
    {

    }
}