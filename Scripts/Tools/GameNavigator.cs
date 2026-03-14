using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Tools;

public static class GameNavigator
{
    private static IReadOnlyGameState? _gameState;
    private static IReadOnlyUnitState? _unit;
    private static readonly GridNavigator GridNavigator = new(CanPass, GetMoveCost, CanBeDestination );

    private static bool CanBeDestination(Vector2I arg)
    {
        if (arg.X < 0 || arg.X >= _gameState!.TileMapState.Size.X || arg.Y < 0 || arg.Y >= _gameState.TileMapState.Size.Y)
            return false;
        var coordsUnit = _gameState.UnitMapState.GetUnit(arg);
        if (_unit!.IsFlying)
        {
            return coordsUnit is null;
        }
        else
        {
            var hasGround = !_gameState.TileMapState.IsEmpty(arg);
            return hasGround && coordsUnit is null;
        }
    }

    private static int GetMoveCost(Vector2I arg) => 1;

    private static bool CanPass(Vector2I arg)
    {
        if (arg.X < 0 || arg.X >= _gameState!.TileMapState.Size.X || arg.Y < 0 || arg.Y >= _gameState.TileMapState.Size.Y)
            return false;
        var coordsUnit = _gameState.UnitMapState.GetUnit(arg);
        if (_unit!.IsFlying)
        {
            return coordsUnit is null || !coordsUnit.IsFlying || coordsUnit.Team == _unit.Team;
        }
        else
        {
            var hasGround = !_gameState.TileMapState.IsEmpty(arg);
            return hasGround && (coordsUnit is null || coordsUnit.IsFlying || coordsUnit.Team == _unit.Team);
        }
    }

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