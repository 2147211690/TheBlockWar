using System.Collections;

namespace TheBlockWar.Scripts.Games.Simulations;

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