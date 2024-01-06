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
        return ParseRequest(request);
    }

    private GraphCreationData ParseRequest(String request)
    {
        var nodesInfo = new List<NodeInfo>();
        var edgesInfo = new List<EdgeInfo>();
        var lines = request.Split("\n");
        foreach (var line in lines)
        {
            var splittedLine = line.Split(":");
            if (splittedLine.Length != 2)
            {
                return null;
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
        private static readonly string FullEdgeInfoPattern = @"^( *\d+ *[eE]\d+ *)+$";
        private static readonly string EdgeInfoPattern = @"^( *\d+ *)+$";
        
        public static EdgesInfo Create(String startNode,
            string stringIncidentNodesData)
        {
            var edgeInfos = new List<EdgeInfo>();
            var stringMatches = SplitFullEdgePattern(stringIncidentNodesData);
            foreach (var elem in stringMatches)
            {
                edgeInfos.Add(EdgeInfo.Create(startNode, elem));
            }
            return new EdgesInfo(edgeInfos);
        }
    }

    private static List<string> SplitFullEdgePattern(string input)
    {
        var res = new List<string>();
        string pattern = @"(\d+)\s*[eE]\s*(\d+)";
        MatchCollection matches = Regex.Matches(input, pattern);
        foreach (Match match in matches)
        {
            res.Add(match.ToString());
        }
        return res;
    }
    
    public record EdgeInfo(string FirstNodeName, string SecondNodeName, int Weight)
    {
        private static readonly string NameWeightPattern = @"^ *(\d+) *[eE](\d+) *$";
        private static readonly string NamePattern = @"^ *(\d+) *$";
        public static EdgeInfo Create(string firstNodeName, string inputString)
        {
            var match = Regex.Match(inputString, NameWeightPattern);
            if (!match.Success)
            {
                match = Regex.Match(inputString, NamePattern);
            }
            if (!match.Success)
            {
                throw new ArgumentException("Incorrect input for node name or weight");
            }
            var nodeId = match.Groups[1].ToString();
            int nodeWeight = match.Groups.Count < 3 ?
                0 : int.Parse(match.Groups[2].ToString());
            return new EdgeInfo(firstNodeName, nodeId, nodeWeight);
        }
    }

    public record NodeInfo(String Name, int Weight)
    {
        private static readonly String FullNameWeightPattern = @"^ *(\d+) *[wW](\d+) *$";
        private static readonly String NameWeightPattern = @"^ *(\d+) *$";
        public static NodeInfo Create(String inputString)
        {
            Match match = Regex.Match(inputString, FullNameWeightPattern);
            if (!match.Success)
            {
                match = Regex.Match(inputString, NameWeightPattern);
            }
            if (!match.Success)
            {
                throw new ArgumentException("Incorrect input for node name or weight");
            }
            var nodeId = match.Groups[1].ToString();
            int nodeWeight = match.Groups.Count < 3 ?
                0 : int.Parse(match.Groups[2].ToString());
            return new NodeInfo(nodeId, nodeWeight);
        }
    }
}
