using System.Collections;
using System.Linq;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimulationResult
{
    public SimulationResult(uint sourceId, TeamTypeEnum teamType, SimulationLayer layer, bool collectSteps = true)
    {
        SourceId = sourceId;
        TeamType = teamType;
        Layer = layer;
        Steps = collectSteps ? new SimStepList() : null;
    }
    public uint SourceId { get; }
    public TeamTypeEnum TeamType { get; }
    public SimulationLayer Layer { get; }
    public IEnumerable<SimStep> MasterSteps => Steps?.Where(a => !a.IsChildStep) ?? [];
    public SimStepList? Steps { get; }
    public SimStep? PeekStep => Steps?.PeekStep;
    
    public override string ToString()
    {
        return $"结果来源于事件:{SourceId} 层:{Layer} 步骤数:{Steps.Count}\n详细步骤:\n{string.Join("\n", Steps)}";
    }
    
    
    public class SimStepList : IReadOnlyList<SimStep>
    {
        public SimStep? PeekStep { get; private set; }
        private readonly List<SimStep> _steps = [];
    
        public void AddStep(SimStep step)
        {
            _steps.Add(step);
            if (!step.IsChildStep) PeekStep = step;
        }
    
        public IEnumerator<SimStep> GetEnumerator()
        {
            return _steps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_steps).GetEnumerator();
        }

        public int Count => _steps.Count;

        public SimStep this[int index] => _steps[index];
    }
}



public enum SimulationLayer
{
    Running,//进行实际的模拟
    Focused,//进行复杂的模拟
    Waiting,//不进行复杂的模拟
}