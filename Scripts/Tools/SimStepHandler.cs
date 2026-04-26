using System.Linq;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public class SimStepHandler
{
    private Func<SimStep, GDTask> _handleAction;
    
    private SimStep _endStep = new SimStep(0);

    public SimStepHandler(Func<SimStep, GDTask> handleAction)
    {
        _handleAction = handleAction;
    }

    public async GDTask Handle(IEnumerable<SimulationContext> results)
    {
        var nextParallel = false;
        var events = new List<object>();
        var stepCollection = new List<SimStep>();
        // 处理为事件列表
        foreach (var step in results.SelectMany(x => x.Steps!.Append(_endStep)))
        {
            if (step.Deep == 0) // 如果是主步骤
            {
                var collection = stepCollection;
                if (collection.Count == 1)
                {
                    var last = collection[0];
                    Func<GDTask> ev = () => _handleAction(last);
                    events.Add(ev);
                    stepCollection.Clear();
                }
                else if (collection.Count > 0)
                {
                    // 一个完整链完成
                    Func<GDTask> ev = async () =>
                    {
                        foreach (var s in collection) await _handleAction(s);
                    };
                    events.Add(ev);
                    stepCollection = [];
                }
            }
            if (step is NextParallelStep) events.Add(step);
            else if (step != _endStep) stepCollection.Add(step);
        }

        var result = new List<Func<GDTask>>();
        var parallelTasks = new List<GDTask>();
        foreach (var eventItem in events)
        {
            if (eventItem is Func<GDTask> func)
            {
                var targetFunc = func;
                if (nextParallel)
                {
                    var prev = result[^1];
                    var targetFuncCopy = targetFunc;
                    targetFunc = () =>
                    {
                        var task = prev();
                        parallelTasks.Add(targetFuncCopy());
                        return task;
                    };
                    result.RemoveAt(result.Count - 1);
                }
                nextParallel = false;
                result.Add(targetFunc);
            }
            else if (eventItem is NextParallelStep parallel) nextParallel = true;
        }

        foreach (var func in result) await func();
        await GDTask.WhenAll(parallelTasks);
    }
}