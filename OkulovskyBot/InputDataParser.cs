namespace OkulovskyBot;

public class InputDataParser
{
    private static String nameWeightPattern = @"^(.)+ [Ww]-?{d+}&";
    
    public void Parse()
    {
        var request = new List<String>();
        request.Add("1 w6 : 2 e7");
        request.Add("2 w5 : 1 e1");
        RequestParser(request);
    }

    private void RequestParser(List<String> request)
    {
        foreach (var line in request)
        {
            var splittedLine = line.Split(":");
            if (splittedLine.Length != 2)
            {
                throw new ArgumentException("Incorrect separator count");
            }
        }
    }
}