using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public abstract class GameMode : IGameMode
{
    protected static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GameMode(){}
    public GameStateEnum State { get; protected set; } = GameStateEnum.Paused;
    
    public abstract void EnterTurn(TurnManager turnManager, int turn);

    public abstract GDTask Turn(TurnManager turnManager, int turn);

    public abstract void ExitTurn(TurnManager turnManager, int turn);
    
    protected async GDTask RunBuffRegionEventAndAddToNextTurnEvent(TurnManager turnManager, int turn, int delay = 800)
    {
        turnManager.GameScheduler.SetNextTurnEventFocused(true);
        await GDTask.Delay(delay);
        turnManager.GameScheduler.RunNextTurnEvent();
        await GDTask.WaitUntil(() => !turnManager.GameView.IsRunning);
        turnManager.GameScheduler.SetNextTurnEvent(GetNextBuffRegionEvent(turn + 1));
    }

    protected SchedulerEvent GetNextBuffRegionEvent(int turn)
    {
        return EventFactory.CreateEvent((state, args, result) =>
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
                                new(coords, tile.Regions.Select(r => Regions.Regions.GetInstance(r.RegionType)).ToArray()));
                    }
                }

                foreach (var pair in regionPairs)
                {
                    foreach (var region in pair.regions)
                    {
                        region.OnEnterTurn(state, pair.coords, turn, result);
                    }
                }
                
                
                List<(IReadOnlyUnitState unit, IBuff[] buffs)> buffPairs =
                    state.UnitMapState.GetAllUnits()
                        .Where(a => a.Buffs.Count > 0)
                        .Select(a => (a, a.Buffs.Select(u => u.BuffType.GetInstance()).ToArray()))
                        .ToList();
                foreach (var pair in buffPairs)
                {
                    foreach (var buff in pair.buffs)
                    {
                        buff.OnEnterTurn(state, pair.unit, turn, result);
                    }
                }
                
                GC.Collect();
            }
        );
    }
}