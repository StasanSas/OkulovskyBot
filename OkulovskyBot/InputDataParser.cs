using System.Text.RegularExpressions;

namespace OkulovskyBot;

public class InputDataParser
{
    private readonly string request;
    
    public InputDataParser(string request)
    {
        this.request = request;
    }
    
    public GraphCreationData Parse()
    {
        return RequestParser(request);
    }

    private GraphCreationData RequestParser(String request)
    {
        var nodesInfo = new List<NodeInfo>();
        var edgesInfo = new List<EdgeInfo>();
        var lines = request.Split("\n");
        foreach (var line in lines)
        {
            var splittedLine = line.Split(":");
            if (splittedLine.Length != 2)
            {
                throw new ArgumentException("Incorrect separator count");
            }
            var nodeInfo = NodeInfo.Create(splittedLine[0]);
            nodesInfo.Add(nodeInfo);
            edgesInfo.AddRange(EdgesInfo.Create(nodeInfo.Name, splittedLine[1]).EdgeInfos);
        }
        return new GraphCreationData(nodesInfo, edgesInfo);
    }

    public record GraphCreationData(List<NodeInfo> NodesInfo,
        List<EdgeInfo> EdgesInfo);

    private record EdgesInfo(List<EdgeInfo> EdgeInfos)
    {
        private static readonly string EdgeInfoPattern = @"^( *\d+ *[eE]\d+ *)+$";
        
        public static EdgesInfo Create(String startNode,
            string stringIncidentNodesData)
        {
            var edgeInfos = new List<EdgeInfo>();
            var match = Regex.Match(stringIncidentNodesData, EdgeInfoPattern);
            var groupsCount = match.Groups.Count;
            for (var i = 1; i < groupsCount; i++)
            {
                edgeInfos.Add(EdgeInfo.Create(startNode, match.Groups[i].ToString()));
            }
            return new EdgesInfo(edgeInfos);
        }
    }
    
    public record EdgeInfo(string FirstNodeName, string SecondNodeName, int? Weight)
    {
        private static readonly string NameWeightPattern = @"^ *(\d+) *[eE](\d+) *$";
        public static EdgeInfo Create(string firstNodeName, string inputString)
        {
            var match = Regex.Match(inputString, NameWeightPattern);
            if (!match.Success)
            {
                throw new ArgumentException("Incorrect input for node name or weight");
            }
            var nodeId = match.Groups[1].ToString();
            int? nodeWeight = match.Groups.Count < 3 ?
                null : int.Parse(match.Groups[2].ToString());
            return new EdgeInfo(firstNodeName, nodeId, nodeWeight);
        }
    }

    public record NodeInfo(String Name, int? Weight)
    {
        private static readonly String NameWeightPattern = @"^ *(\d+) *[wW](\d+) *$";
        public static NodeInfo Create(String inputString)
        {
            Match match = Regex.Match(inputString, NameWeightPattern);
            if (!match.Success)
            {
                throw new ArgumentException("Incorrect input for node name or weight");
            }
            var nodeId = match.Groups[1].ToString();
            int? nodeWeight = match.Groups.Count < 3 ?
                null : int.Parse(match.Groups[2].ToString());
            return new NodeInfo(nodeId, nodeWeight);
        }
    }
}
