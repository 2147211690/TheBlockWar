using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Tools;

public static class GameNavigator
{
    private static IReadOnlyGameState? _gameState;
    private static IReadOnlyUnitState? _unit;
    private static readonly GridNavigator GridNavigator = new(CanPass, GetMoveCost, CanBeDestination );

    public static bool IsCanBeDestination(this IReadOnlyUnitState unit, IReadOnlyGameState state, in Vector2I coords)
    {
        if (coords.X < 0 || coords.X >= state.TileMapState.Size.X || coords.Y < 0 || coords.Y >= state.TileMapState.Size.Y)
            return false;
        var coordsUnit = state.UnitMapState.GetUnit(coords);
        if (unit.IsFlying)
        {
            return coordsUnit.IsNull();
        }
        else
        {
            var hasGround = !state.TileMapState.IsEmpty(coords);
            return hasGround && coordsUnit.IsNull();
        }
    }

    public static bool IsCanPass(this IReadOnlyUnitState unit, IReadOnlyGameState state, in Vector2I coords)
    {
        if (coords.X < 0 || coords.X >= state.TileMapState.Size.X || coords.Y < 0 || coords.Y >= state.TileMapState.Size.Y)
            return false;
        var coordsUnit = state.UnitMapState.GetUnit(coords);
        if (unit.IsFlying)
        {
            return coordsUnit.IsNull() || !coordsUnit.IsFlying ||
                   state.TeamState.IsPlayerOrFriend(coordsUnit.Team, unit.Team);
        }
        else
        {
            var hasGround = !state.TileMapState.IsEmpty(coords);
            return hasGround && 
                   (coordsUnit.IsNull() || coordsUnit.IsFlying || state.TeamState.IsPlayerOrFriend(coordsUnit.Team, unit.Team));
        }
    }

    public static bool IsWillDrop(this IReadOnlyUnitState unit, IReadOnlyGameState state, in Vector2I coords)
    {
        if (!state.IsInMap(coords)) return true;
        var hasGround = !state.TileMapState.IsEmpty(coords);
        return !hasGround && !unit.IsFlying;
    }
    private static bool CanBeDestination(Vector2I arg) => _unit!.IsCanBeDestination(_gameState!, arg);

    private static int GetMoveCost(Vector2I arg) => 1;

    private static bool CanPass(Vector2I arg) => _unit!.IsCanPass(_gameState!, arg);

    public static List<Vector2I> FindShortestPath(IReadOnlyGameState gameState, IReadOnlyUnitState unit, Vector2I start, Vector2I end)
    {
        _gameState = gameState;
        _unit = unit;
        var r = GridNavigator.FindShortestPath(start, end);
        _gameState = null;
        _unit = null;
        return r;
    }

    public static List<Vector2I> FindValidDestinationsWithinCost(
        IReadOnlyGameState gameState, IReadOnlyUnitState unit, Vector2I start, int maxCost)
    {
        _gameState = gameState;
        _unit = unit;
        var r = GridNavigator.FindValidDestinationsWithinCost(start, maxCost);
        _gameState = null;
        _unit = null;
        return r;
    }
}