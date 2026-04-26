using System.Collections;
using System.Linq;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimulationContext
{
    public SimulationContext(bool collectSteps = true)
    {
        Steps = collectSteps ? new SimStepList() : null;
    }
    public required long SourceId { get; init;}
    public required TeamTypeEnum TeamType { get; init;}
    public required SimulationLayer Layer { get; init;}
    public long SourceUnitId { get; init; } = 0;
    public IEnumerable<SimStep> MasterSteps => Steps?.Where(a => a.Deep == 0) ?? [];
    public SimStepList? Steps { get; }
    public SimStep? PeekStep => Steps?.PeekStep;
    
    public override string ToString()
    {
        return $"\n上下文:事件源:{SourceId} 队伍:{TeamType} 层:{Layer} 单位源:{SourceUnitId} 步骤数:{Steps?.Count ?? 0}\n详细步骤:\n{string.Join("\n", Steps ?? [])}";
    }
    
    
    public class SimStepList : IReadOnlyList<SimStep>
    {
        public SimStep? PeekStep { get; private set; }
        private readonly List<SimStep> _steps = [];
    
        public void AddStep(SimStep step)
        {
            _steps.Add(step);
            if (step.Deep == 0) PeekStep = step;
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
    Disabled,//已禁用
}