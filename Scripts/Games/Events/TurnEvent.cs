using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class TurnEvent(long id) : SchedulerEvent(id)
{
    public TypeEnum TurnType { get; set => SetField(ref field, value); }

    public override void OnApply(SchedulerEvent root, SimObject obj)
    {
        if (TurnType == TypeEnum.Region)
        {
            var regionPairs = new List<(Vector2I coords, List<string> regions)>();
            var size = obj.State.TileMapState.Size;
            for (var i = 0; i < size.X; i++)
            {
                for (var j = 0; j < size.Y; j++)
                {
                    var coords = new Vector2I(i, j);
                    var tile = obj.State.TileMapState.GetTile(coords);
                    if (tile.Name.GetTileInfo().Regions is {Length: > 0} && tile.Regions.Count > 0)
                        regionPairs.Add(
                            new(coords, tile.GetInfoAndCurrentRegions().ToList()));
                }
            }

            foreach (var pair in regionPairs)
            {
                foreach (var region in pair.regions)
                {
                    region.GetRegionInstance().OnEnterTurn(obj, pair.coords, Turn);
                }
            }

            foreach (var pair in regionPairs)
            {
                foreach (var region in pair.regions)
                {
                    var tile = obj.State.TileMapState.GetTile(pair.coords);
                    if (tile.GetRegionDuration(region) is 0 or int.MaxValue) continue;
                    obj.ApplyModifyTileRegion(pair.coords, region, RegionOperation.ChangeDuration, -1);
                }
            }
        }
        else if (TurnType == TypeEnum.Buff)
        {
            List<(IReadOnlyUnitState unit, List<string> buffs)> buffPairs =
                obj.State.UnitMapState.GetAllUnits().Where(
                        u => u.Name.GetUnitInfo().Buffs is {Length: > 0} && u.Buffs.Count > 0
                        )
                    .Select(
                        u => (u, u.GetInfoAndCurrentBuffs(obj.State).ToList())
                            ).ToList();
            foreach (var pair in buffPairs)
            {
                foreach (var buff in pair.buffs)
                {
                    buff.GetBuffInstance().OnEnterTurn(obj, pair.unit, Turn);
                }
            }
            foreach (var pair in buffPairs)
            {
                foreach (var buff in pair.buffs)
                {
                    if (pair.unit.GetBuffDuration(buff) is 0 or int.MaxValue) continue;
                    obj.ApplyModifyUnitBuff(pair.unit.Coords, buff, BuffOperation.ChangeDuration, -1);
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