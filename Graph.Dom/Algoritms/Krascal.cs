using Graph.Inf;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static Color DefineColor(State elem)
        {
            switch (elem)
            {
                case State.White: return Color.FloralWhite;
                case State.Gray: return Color.Gray;
                case State.Black: return Color.Red;
                default: return Color.Red;
            }
        }

        public static Graph<string, int, State> Algoritm(Graph<string, int, State> fullGraph)
        {
            var edges = fullGraph.Edges;
            var tree = new Graph<string, int, State>(true);
            foreach(var node in fullGraph.Nodes)
            {
                tree.Nodes.AddNode(node);
            }
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
            var notVisited = new HashSet<NodeVisual<string, int, State>>(graph.Nodes);
            while (notVisited.Count!=0)
            {
                var stack = new Stack<NodeVisual<string, int, State>>();
                var startNode = notVisited.ElementAt(0);

                startNode.State = State.Gray;
                stack.Push(startNode);
                while (stack.Count != 0)
                {
                    var node = stack.Pop();
                    foreach (var edge in node.AdjacentEdges)
                    {
                        NodeVisual<string, int, State> nextNode;
                        if (edge.Start == node)
                            nextNode = edge.End;
                        else
                            nextNode = edge.Start;

                        if (nextNode.State == State.Black) continue;
                        if (nextNode.State == State.Gray) 
                            return true;
                        nextNode.State = State.Gray;
                        stack.Push(nextNode);
                    }
                    node.State = State.Black;
                    notVisited.Remove(node);
                }
            }            
            return false;
        }

        public static List<Change<List<string>, int, State>> GetObserverForGraph(Graph<string, int, State> graph)
        {
            return Algoritm(graph).GetChanges();
        }
    }
}
