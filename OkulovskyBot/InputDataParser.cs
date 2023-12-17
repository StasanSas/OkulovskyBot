using System.Text.RegularExpressions;

namespace OkulovskyBot;

public class InputDataParser
{
    public void Parse()
    {
        var request = "1 w6 : 2 e7\n2 w5 : 1 e1";
        RequestParser(request);
    }

    private void RequestParser(String request)
    {
        var lines = request.Split("\n");
        foreach (var line in lines)
        {
            var splittedLine = line.Split(":");
            if (splittedLine.Length != 2)
            {
                throw new ArgumentException("Incorrect separator count");
            }
            var nodeInfo = NodeInfo.Create(splittedLine[0]);
            var edgesInfo = EdgesInfo.Create(nodeInfo.Name, splittedLine[1]);
            var a = 1 + 1;
        }
    }

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
    
    private record EdgeInfo(string firstNodeName, string secondNodeName, int? Weight)
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

    private record NodeInfo(String Name, int? Weight)
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