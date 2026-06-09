using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyBuffTurnResult ApplyBuffTurn(int turn)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyBuffTurnResult Main()
            {
                ApplyBuffTurnResult r = obj.Steps.AddResult(new ApplyBuffTurnResult(CurrentDeep, turn));
                if (obj.Layer == SimulationLayer.Waiting) return r;
                List<(long id, IReadOnlyList<BuffState> buffs)> buffPairs =
                    obj.State.UnitMapState.GetAllUnits().Where(u => u is { IsDead: false, Buffs.Count: > 0 }
                        )
                        .Select(u => (u.Id, u.BuffsStatic)
                        )
                        .ToList();
                obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
                foreach (var pair in buffPairs)
                {
                    Hook.BuffEnterTurn(obj.State.UnitMapState.GetUnit(pair.id), obj, turn);
                }

                foreach (var pair in buffPairs)
                {
                    var unit = obj.State.UnitMapState.GetUnit(pair.id);
                    foreach (var buff in pair.buffs)
                    {
                        var info = buff.BuffName.GetBuffInfo();
                        if (unit.GetBuffCount(buff.BuffName) is 0 or int.MaxValue || info.TurnRemoveCount <= 0)
                            continue;
                        obj.ApplyModifyUnitBuff(unit.Coords, buff.BuffName, BuffOperation.ChangeDuration,
                            -info.TurnRemoveCount);
                    }
                }
                obj.EndSteps();
                return r;
            }
        }
    }

    public static partial void ApplyBuffTurnViewDraw(GameView view, ReadOnlySimObject sim, ApplyBuffTurnResult result)
    {
    }

    public static partial GDTask ApplyBuffTurnViewRun(GameView view, ReadOnlySimObject sim, ApplyBuffTurnResult result)
    {
        return GDTask.CompletedTask;
    }
}