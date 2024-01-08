using Graph.Dom;

namespace OkulovskyBot;

public static class GraphCreator
{
    public static Graph<string, int, TState> GetParsedGraph<TState>(string inputGraphInfo, bool withObserver = false)
    {
        var graph = new Graph<string, int, TState>(withObserver);
        var graphData = new InputDataParser(inputGraphInfo).Parse();
        if (graphData == null)
        {
            return null;
        }
        foreach (var nodeInfo in graphData.NodesInfo)
        {
            graph.Nodes.AddNode(new NodeVisual<string, int, TState>(nodeInfo.Name, nodeInfo.Weight));
        }
        var writtenNodes = graph.Nodes.Select(x => x.Id).ToHashSet();
        foreach (var edgeInfo in graphData.EdgesInfo)
        {
            if (!writtenNodes.Contains(edgeInfo.SecondNodeName))
            {
                graph.Nodes.AddNode(new NodeVisual<string, int, TState>(edgeInfo.SecondNodeName, 0));
            }
        }
        foreach (var edgeInfo in graphData.EdgesInfo)
        {
            graph.Edges.AddEdge(graph.Nodes[edgeInfo.FirstNodeName]
                .Connect(graph.Nodes[edgeInfo.SecondNodeName], edgeInfo.Weight));
        }
        return graph;
    }
}