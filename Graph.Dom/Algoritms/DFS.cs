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
    public enum StateDFS
    {
        White,
        Green,
        Yellow
    }


    public static class DFS
    {

        public static Color DefineColor(StateDFS elem)
        {
            switch (elem)
            {
                case StateDFS.White: return Color.FloralWhite;
                case StateDFS.Green: return Color.Green;
                case StateDFS.Yellow: return Color.Yellow;
                default: return Color.Red;
            }
        }

        public static Graph<string, int, StateDFS> Algoritm(Graph<string, int, StateDFS> fullGraph, NodeVisual<string, int, StateDFS> start)
        {
            start.State = StateDFS.Green;
            foreach(var edge in start.AdjacentEdges)
            {
                NodeVisual<string, int, StateDFS> nextNode;
                if (edge.Start == start)
                    nextNode = edge.End;
                else
                    nextNode = edge.Start;

                if (nextNode.State != StateDFS.Green)
                    Algoritm(fullGraph, nextNode);
            }
            start.State = StateDFS.White;
            return fullGraph;
        }

        public static List<Change<List<string>, int, StateDFS>> GetObserverForGraph(Graph<string, int, StateDFS> graph, NodeVisual<string, int, StateDFS> start)
        {
            return Algoritm(graph, start).GetChanges();
        }
    }
}
