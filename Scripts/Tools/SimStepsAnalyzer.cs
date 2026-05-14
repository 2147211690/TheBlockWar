using System.Linq;
using System.Text;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Tools;

public static class SimStepsAnalyzer
{
    public static TreeNode ToTree(this IEnumerable<SimStep> steps)
    {
        var root = new TreeNode(new SimStep(null, -1));
        var stack = new Stack<TreeNode>();
        stack.Push(root);

        foreach (var step in steps)
        {
            var node = new TreeNode(step);
            while (stack.Peek().Deep >= step.Deep) stack.Pop();
            stack.Peek().Children.Add(node);
            stack.Push(node);
        }
        return root;
    }
    
    
    
    public struct TreeNode(SimStep step)
    {
        public SimStep Step { get; set; } = step;
        public List<TreeNode> Children { get; set; } = [];
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
