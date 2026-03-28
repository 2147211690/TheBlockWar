using System.Linq;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public class SimStepHandler
{
    private Func<SimStep, GDTask> _handleAction;
    
    private SimStep _endStep = new SimStep(false);

    public SimStepHandler(Func<SimStep, GDTask> handleAction)
    {
        _handleAction = handleAction;
    }

    public async GDTask Handle(IEnumerable<SimulationResult> results)
    {
        var nextParallel = false;
        var nextDelay = 0;
        var events = new List<object>();
        var stepCollection = new List<SimStep>();
        // 处理为事件列表
        foreach (var step in results.SelectMany(x => x.Steps!.Append(_endStep)))
        {
            if (!step.IsChildStep) // 如果是主步骤
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
            if (step is ParallelStep or DelayStep) events.Add(step);
            else if (step != _endStep) stepCollection.Add(step);
        }

        var result = new List<Func<GDTask>>();
        var parallelTasks = new List<GDTask>();
        foreach (var eventItem in events)
        {
            if (eventItem is Func<GDTask> func)
            {
                var targetFunc = func;
                var delay = nextDelay;
                if (delay != 0)
                {
                    targetFunc = async () =>
                    {
                        await GDTask.Delay(delay);
                        await func();
                    };
                }
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
                nextDelay = 0;
                nextParallel = false;
                result.Add(targetFunc);
            }
            else
            {
                if (eventItem is DelayStep delay) nextDelay = delay.Delay;
                else if (eventItem is ParallelStep parallel) nextParallel = true;
            }
        }

        foreach (var func in result) await func();
        await GDTask.WhenAll(parallelTasks);
    }
}