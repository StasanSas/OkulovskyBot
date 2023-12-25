using Graph.Dom;
using Graph.Dom.Algoritms;

namespace OkulovskyBot;

public static class GraphCreator
{
    public static Graph<string, int, N> GetParsedGraph(string inputGraphInfo)
    {
        var graph = new Graph<string, int, N>();
        var graphData = new InputDataParser(inputGraphInfo).Parse();
        foreach (var nodeInfo in graphData.NodesInfo)
        {
            graph.Nodes.AddNode(new NodeVisual<string, int, N>(nodeInfo.Name, nodeInfo.Weight));
        }
        foreach (var edgeInfo in graphData.EdgesInfo)
        {
            graph.Edges.AddEdge(graph.Nodes[edgeInfo.FirstNodeName]
                .Connect(graph.Nodes[edgeInfo.SecondNodeName], edgeInfo.Weight));
        }
        return graph;
    }
}