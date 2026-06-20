using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;
/// <summary>
/// 为了保证可预知性，模拟结果中的步骤必须按照顺序应用到游戏状态上
/// 任何对游戏的操作应该使用Event, 而不是直接修改游戏状态
/// Waiting层的模拟结果不应该主要用来标记,而不具体修改数据
/// </summary>
public static partial class Simulator
{
    /// <summary>
    /// 是否进行复杂模拟
    /// </summary>
    public static int ComplexSimulationMaxLevel { get; set; } = int.MaxValue;
    private static int _simulationStack = -1;
    private static short CurrentDeep => (short)_simulationStack;
    private static int CurrentTeam { get; set; }
    private static void EnterSimulation()
    {
        if (_simulationStack == -1)
        {
            CurrentComplexSimulationMaxLevel = ComplexSimulationMaxLevel;
        }
        _simulationStack++;
    }
    private static void ExitSimulation() => _simulationStack--;
    private static int CurrentComplexSimulationMaxLevel { get; set; }

    public static SimContext PreviousContext => _contexts.Count > 2 ? _contexts[^2] : new SimContext(0);
    public static SimContext CurrentContext => _contexts[^1];
    
    private static List<SimContext> _contexts = new([new SimContext(0)]);
    extension(SimObject obj)
    {
        public void PushContext(SimContext context)
        {
            var previous = CurrentContext;
            _contexts.Add(context);
            if (_contexts.Count == 2)
                obj.UpdateCurrentTeamType(context.SourceUnitId);
            else if (context != previous)
                obj.UpdateCurrentTeamType(context.SourceUnitId);
        }
        public SimContext PopContext()
        {
            if (_contexts.Count == 1) return CurrentContext;
            var previous = _contexts[^1];
            _contexts.RemoveAt(_contexts.Count - 1);
            var current = _contexts[^1];
            if (current != previous)
                obj.UpdateCurrentTeamType(current.SourceUnitId);
            return previous;
        }
        private void UpdateCurrentTeamType(in long sourceUnitId)
        {
            if (sourceUnitId == 0) CurrentTeam = -1;
            else CurrentTeam = obj.State.UnitMapState.GetUnit(sourceUnitId).Team;
        }
        private bool ComplexSimulationRegion(int level) 
            => CurrentComplexSimulationMaxLevel >= level || obj.Layer == SimulationLayer.Running;
        /// <summary>
        /// 开始封装一系列步骤, 请记住调用EndSteps结束
        /// </summary>
        public void BeginSteps(int startDelay, int endDelay, StepsRunningMode runningMode)
        {
            if (obj.Layer != SimulationLayer.Running) return;
            EnterSimulation();
            obj.Steps.AddResult(new ApplyStepsResult(CurrentDeep, CurrentTeam, [], startDelay, endDelay, runningMode));
        }
        /// <summary>
        /// 结束封装一系列步骤
        /// </summary>
        public void EndSteps()
        {
            if (obj.Layer != SimulationLayer.Running) return;
            ExitSimulation();
        }
    }
}