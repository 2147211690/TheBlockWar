using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        /// <summary>
        /// 用来标记下次动画并行执行
        /// </summary>
        public void ApplyNextParallel(SimulationContext context)
        {
            if (context.Layer == SimulationLayer.Running && _simulationStack == 0) 
                context.Steps?.AddStep(new NextParallelStep(false));
        }
    }
}