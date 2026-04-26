using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools.Jsons;

[JsonSerializable(typeof(TileMapStateData))]
[JsonSerializable(typeof(TileStateData))]
[JsonSerializable(typeof(UnitStateData))]
[JsonSerializable(typeof(UnitMapStateData))]
[JsonSerializable(typeof(TeamStateData))]
[JsonSerializable(typeof(GameStateData))]
[JsonSerializable(typeof(SchedulerEventListData))]
[JsonSerializable(typeof(SchedulerEvent))]
[JsonSerializable(typeof(AnimationType))]
[JsonSerializable(typeof(ApplyStep[]))]
[JsonSourceGenerationOptions(IncludeFields = true, WriteIndented = true)]
public partial class GameJsonSerializerContext : JsonSerializerContext
{
}