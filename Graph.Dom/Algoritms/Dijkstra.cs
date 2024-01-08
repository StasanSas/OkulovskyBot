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


    public enum N
    {
        None = 0,
        Track = 1,
    }


    public static class Dijkstra
    {
        public static Color DefineColor(N elem)
        {
            switch (elem)
            {
                case N.None : return Color.FloralWhite;
                case N.Track : return Color.Red;
                default: return Color.White;
            }
        }
        
        public static Graph<string, int, N>  Algoritm(Graph<string, int, N> graph, string start, string end)
        {
            var notVisited = graph.Nodes.ToList();
            var track = new Dictionary<NodeVisual<string, int, N>, NodeVisual<string, int, N>>();
            track[graph.Nodes[start]] = null;
            graph.Nodes[start].State = N.Track;

            while (true)
            {
                NodeVisual<string, int, N>? toOpen = null;
                var bestPrice = double.PositiveInfinity;
                foreach (var e in notVisited)
                {
                    if (e.State == N.Track && e.Weight < bestPrice)
                    {
                        bestPrice = e.Weight;
                        toOpen = e;
                    }
                }

                if (toOpen == null) return graph;
                if (toOpen.Id == end) break;

                foreach (var e in toOpen.AdjacentEdges)
                {
                    var currentPrice = toOpen.Weight + e.Weight;
                    var nextNode = e.End;
                    if (nextNode.State == N.None || nextNode.Weight > currentPrice)
                    {
                        track[nextNode] = toOpen;
                        nextNode.State = N.Track; 
                        nextNode.Weight = currentPrice;
                    }
                }

                notVisited.Remove(toOpen);
            }

            var result = new List<NodeVisual<string, int, N>>();
            var endNode = graph.Nodes[end];
            while (endNode != null)
            {
                endNode.GetSignal(TypeChange.UnidentifiedNode);
                result.Add(endNode);
                endNode = track[endNode];               
            }
            result.Reverse();
            return graph;
        }

        public static List<Change<List<string>, int, N>> GetObserverForGraph(Graph<string, int, N> graph, string start, string end)
        {           
            return Algoritm(graph, start, end).GetChanges();
        }
    }
    
}
