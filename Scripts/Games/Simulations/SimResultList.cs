using System.Collections;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimResultList : IReadOnlyList<StepResult>
{
    public StepResult? PeekStep { get; private set; }
    private readonly List<StepResult> _steps = [];
    
    public void AddResult(StepResult step)
    {
        _steps.Add(step);
        if (step.Deep == 0) PeekStep = step;
    }
    
    public IEnumerator<StepResult> GetEnumerator()
    {
        return _steps.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_steps).GetEnumerator();
    }

    public int Count => _steps.Count;

    public StepResult this[int index] => _steps[index];
}