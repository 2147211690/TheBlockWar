using System.Linq;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public class SimResultHandler
{
    private Func<ReadOnlySimObject, StepResult, GDTask> _handleAction;

    public SimResultHandler(Func<ReadOnlySimObject, StepResult, GDTask> handleAction)
    {
        _handleAction = handleAction;
    }

    public async GDTask HandleAsync(IEnumerable<ReadOnlySimObject> sims)
    {
        var tree = sims.ToTree();   // 根节点 Deep = -1
        await ProcessNodeAsync(tree);
    }

    private async GDTask ProcessNodeAsync(SimStepsAnalyzer.TreeNode node)
    {
        var step = node.Step;

        if (step is ApplyStepsResult stepsStep)
        {
            await ProcessStepsStepAsync(stepsStep, node.Children);
            return;
        }

        await _handleAction(node.SimObject!, step);

        foreach (var child in node.Children)
            await ProcessNodeAsync(child);
    }

    /// <summary>
    /// 处理 StepsStep：先 StartDelay，再按模式执行所有子节点，最后 EndDelay
    /// </summary>
    private async GDTask ProcessStepsStepAsync(ApplyStepsResult stepsStep, List<SimStepsAnalyzer.TreeNode> children)
    {
        // 前置延迟
        if (stepsStep.StartDelay > 0) await GDTask.Delay(stepsStep.StartDelay);

        // 根据运行模式执行子节点
        switch (stepsStep.RunningMode)
        {
            case StepsRunningMode.QueueWaitAll:
                // 顺序执行，每个等待完成
                foreach (var child in children)
                    await ProcessNodeAsync(child);
                break;

            case StepsRunningMode.QueueWithoutWait:
                // 顺序启动，但不等待完成（并行执行，但启动顺序保证）
                Main().Forget();
                break;

                async GDTask Main()
                {
                    foreach (var child in children) await ProcessNodeAsync(child);
                }

            case StepsRunningMode.ParallelWaitAll:
                // 并行执行，等待所有完成
                await GDTask.WhenAll(children.Select(ProcessNodeAsync));
                break;

            case StepsRunningMode.ParallelWaitAny:
                // 并行执行，等待任意一个完成
                await GDTask.WhenAny(children.Select(ProcessNodeAsync));
                break;

            case StepsRunningMode.ParallelWithoutWait:
                // 并行启动，不等待任何完成
                foreach (var child in children) ProcessNodeAsync(child).Forget();
                break;
        }
        // 后置延迟
        if (stepsStep.EndDelay > 0) await GDTask.Delay(stepsStep.EndDelay);
    }
}