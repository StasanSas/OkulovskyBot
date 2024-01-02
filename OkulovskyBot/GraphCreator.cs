using Graph.Dom;

namespace OkulovskyBot;

public static class GraphCreator
{
    public static Graph<string, int, TState> GetParsedGraph<TState>(string inputGraphInfo)
    {
        var graph = new Graph<string, int, TState>();
        var graphData = new InputDataParser(inputGraphInfo).Parse();
        foreach (var nodeInfo in graphData.NodesInfo)
        {
            graph.Nodes.AddNode(new NodeVisual<string, int, TState>(nodeInfo.Name, nodeInfo.Weight));
        }
        foreach (var edgeInfo in graphData.EdgesInfo)
        {
            graph.Edges.AddEdge(graph.Nodes[edgeInfo.FirstNodeName]
                .Connect(graph.Nodes[edgeInfo.SecondNodeName], edgeInfo.Weight));
        }
        return graph;
    }
}