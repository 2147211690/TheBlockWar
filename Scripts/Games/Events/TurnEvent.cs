using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class TurnEvent(long id) : SchedulerEvent(id)
{
    public TypeEnum TurnType { get; set => SetField(ref field, value); }

    public override void OnApply(SchedulerEvent root, IGameState state, SimulationContext context)
    {
        if (TurnType == TypeEnum.Region)
        {
            var regionPairs = new List<(Vector2I coords, IRegion[] regions)>();
            var size = state.TileMapState.Size;
            for (var i = 0; i < size.X; i++)
            {
                for (var j = 0; j < size.Y; j++)
                {
                    var coords = new Vector2I(i, j);
                    var tile = state.TileMapState.GetTile(coords);
                    if (tile.Regions.Count > 0)
                        regionPairs.Add(
                            new(coords,
                                tile.Regions.Select(r => r.RegionType.GetInstance()).ToArray()));
                }
            }

            foreach (var pair in regionPairs)
            {
                foreach (var region in pair.regions)
                {
                    region.OnEnterTurn(state, pair.coords, Turn, context);
                }
            }
        }
        else if (TurnType == TypeEnum.Buff)
        {
            List<(IReadOnlyUnitState unit, IBuff[] buffs)> buffPairs =
                state.UnitMapState.GetAllUnits()
                    .Where(a => a.Buffs.Count > 0)
                    .Select(a => (a, a.Buffs.Select(u => u.BuffType.GetInstance()).ToArray()))
                    .ToList();
            foreach (var pair in buffPairs)
            {
                foreach (var buff in pair.buffs)
                {
                    buff.OnEnterTurn(state, pair.unit, Turn, context);
                }
            }
        }
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> TurnEvent 类型:{TurnType}";
    }

    public enum TypeEnum
    {
        Buff,
        Region,
    }
}