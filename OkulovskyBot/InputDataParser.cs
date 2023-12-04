namespace OkulovskyBot;

public class InputDataParser
{
    private static String nameWeightPattern = @"^(.)+ [Ww]-?{d+}&";
    
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
        }
    }
}