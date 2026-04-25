using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Tools.Jsons;

[JsonSerializable(typeof(TileMapStateData))]
[JsonSerializable(typeof(TileStateData))]
[JsonSerializable(typeof(UnitStateData))]
[JsonSerializable(typeof(UnitMapStateData))]
[JsonSerializable(typeof(TeamStateData))]
[JsonSerializable(typeof(GameStateData))]
[JsonSourceGenerationOptions(IncludeFields = true, WriteIndented = true)]
public partial class GameJsonSerializerContext : JsonSerializerContext
{
}