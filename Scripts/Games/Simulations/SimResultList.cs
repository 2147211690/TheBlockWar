using System.Collections;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimResultList : IReadOnlyList<StepResult>
{
    private readonly List<StepResult> _steps = [];
    
    public T AddResult<T>(T step) where T : StepResult
    {
        _steps.Add(step);
        return step;
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