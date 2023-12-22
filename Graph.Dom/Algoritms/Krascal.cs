using Graph.Inf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Graph.Dom.Algoritms
{
    public enum State
    {
        White,
        Gray,
        Black
    }

    public static class Krascal
    {

        public static Graph<string, int, State> Algoritm(Graph<string, int, State> fullGraph)
        {
            var edges = fullGraph.Edges;
            var tree = new Graph<string, int, State>(true);
            foreach (var edge in edges.OrderBy(x => x.Weight))
            {
                tree.Edges.AddEdge(edge);
                if (Krascal.HasCycle(tree))
                    tree.Edges.RemoveEdge(edge);
                foreach(var node in tree.Nodes)
                    node.State = State.White;
            }
            return tree;
        }


        public static bool HasCycle(Graph<string, int, State> graph)
        {
            var stack = new Stack<NodeVisual<string, int, State>>();
            var startNode = graph.Nodes.First();
            startNode.State = State.Gray;
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                foreach (var nextNode in node.AdjacentEdges)
                {
                    if (nextNode.End.State == State.Black) continue;
                    if (nextNode.End.State == State.Gray) return true;
                    nextNode.End.State = State.Gray;
                    stack.Push(nextNode.End);
                }
                node.State = State.Black;
            }
            return false;
        }

        public static List<Change<List<string>, int, State>> GetObserverForGraph(Graph<string, int, State> graph)
        {
            return Algoritm(graph).GetChanges();
        }
    }
}
