using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        /// <summary>
        /// 用来标记下次动画并行执行
        /// </summary>
        public void NextParallel(SimulationResult result)
        {
            if (result.Layer == SimulationLayer.Running && _simulationStack == 0) 
                result.Steps?.AddStep(new ParallelStep(false));
        }
    }
}