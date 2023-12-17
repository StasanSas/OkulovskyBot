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
            // var edgesInfo = 
            var a = 1 + 1;
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
                throw new AggregateException("Incorrect input for node name or weight");
            }
            var nodeId = match.Groups[1].ToString();
            int? nodeWeight = match.Groups.Count < 3 ?
                null : int.Parse(match.Groups[2].ToString());
            return new NodeInfo(nodeId, nodeWeight);
        }
    }
}