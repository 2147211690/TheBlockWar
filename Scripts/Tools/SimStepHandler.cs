using System.Linq;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public class SimStepHandler
{
    private Func<SimStep, GDTask> _handleAction;

    public SimStepHandler(Func<SimStep, GDTask> handleAction)
    {
        _handleAction = handleAction;
    }

    public async GDTask Handle(IEnumerable<SimStep> steps)
    {
        var tree = steps.ToTree();   // 根节点 Deep = -1
        await ProcessNode(tree);
    }

    private async GDTask ProcessNode(SimStepsAnalyzer.TreeNode node)
    {
        var step = node.Step;

        if (step is StepsStep stepsStep)
        {
            await ProcessStepsStep(stepsStep, node.Children);
            return;
        }

        await _handleAction(step);

        foreach (var child in node.Children)
            await ProcessNode(child);
    }

    /// <summary>
    /// 处理 StepsStep：先 StartDelay，再按模式执行所有子节点，最后 EndDelay
    /// </summary>
    private async GDTask ProcessStepsStep(StepsStep stepsStep, List<SimStepsAnalyzer.TreeNode> children)
    {
        // 前置延迟
        if (stepsStep.StartDelay > 0) await GDTask.Delay(stepsStep.StartDelay);

        // 根据运行模式执行子节点
        switch (stepsStep.RunningMode)
        {
            case StepsRunningMode.QueueWaitAll:
                // 顺序执行，每个等待完成
                foreach (var child in children)
                    await ProcessNode(child);
                break;

            case StepsRunningMode.QueueWithoutWait:
                // 顺序启动，但不等待完成（并行执行，但启动顺序保证）
                Main().Forget();
                break;

                async GDTask Main()
                {
                    foreach (var child in children) await ProcessNode(child);
                }

            case StepsRunningMode.ParallelWaitAll:
                // 并行执行，等待所有完成
                await GDTask.WhenAll(children.Select(ProcessNode));
                break;

            case StepsRunningMode.ParallelWaitAny:
                // 并行执行，等待任意一个完成
                await GDTask.WhenAny(children.Select(ProcessNode));
                break;

            case StepsRunningMode.ParallelWithoutWait:
                // 并行启动，不等待任何完成
                foreach (var child in children) ProcessNode(child).Forget();
                break;
        }
        // 后置延迟
        if (stepsStep.EndDelay > 0) await GDTask.Delay(stepsStep.EndDelay);
    }
}