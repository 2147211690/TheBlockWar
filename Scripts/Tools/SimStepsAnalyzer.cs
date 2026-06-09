using System.Linq;
using System.Text;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public static class SimStepsAnalyzer
{
    public static TreeNode ToTree(this IEnumerable<ReadOnlySimObject> sims)
    {
        var root = new TreeNode(null, new StepResult(null, -1));
        var stack = new Stack<TreeNode>();
        stack.Push(root);

        foreach (var sim in sims)
        {
            foreach (var step in sim.Steps ?? [])
            {
                var node = new TreeNode(sim, step);
                while (stack.Peek().Deep >= step.Deep) stack.Pop();
                stack.Peek().Children.Add(node);
                stack.Push(node);
            }
        }
        return root;
    }

    public static TreeNode ToTree(this IEnumerable<StepResult> steps)
    {
        var root = new TreeNode(null, new StepResult(null, -1));
        var stack = new Stack<TreeNode>();
        stack.Push(root);
        foreach (var step in steps)
        {
            var node = new TreeNode(null, step);
            while (stack.Peek().Deep >= step.Deep) stack.Pop();
            stack.Peek().Children.Add(node);
            stack.Push(node);
        }
        return root;
    }


    public class TreeNode(ReadOnlySimObject? sim, StepResult step)
    {
        public ReadOnlySimObject? SimObject { get; set; } = sim;
        public StepResult Step { get; set; } = step;
        public List<TreeNode> Children { get => field ??= []; set; }
        public int Deep => Step.Deep;
        /// <summary>
        /// 可视化树形图
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            BuildTreeString(sb, "", true);
            return sb.ToString();
        }

        private void BuildTreeString(StringBuilder sb, string prefix, bool isLast)
        {
            sb.Append(prefix);
            sb.Append(isLast ? "└─ " : "├─ ");
            sb.AppendLine(Step.ToString());

            string childPrefix = prefix + (isLast ? "   " : "│  ");
            for (int i = 0; i < Children.Count; i++)
            {
                bool lastChild = (i == Children.Count - 1);
                Children[i].BuildTreeString(sb, childPrefix, lastChild);
            }
        }
    }
}
