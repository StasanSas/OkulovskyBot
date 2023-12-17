namespace TestProject;
using OkulovskyBot;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void TestNameWeightPattern()
    {
        var graph = GraphCreator.GetParsedGraph("1 w6 : 2 e7\n2 w5 : 1 e1");
    }
}