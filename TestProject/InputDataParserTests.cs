namespace TestProject;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void FullNameWeightPatternTest()
    {
        var graph = GraphCreator.GetParsedGraph("1 w6 : 2 e7\n2 w5 : 1 e1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(6));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(5));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(7));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(1));
    }
    
    [Test]
    public void OnlyNodeWeightTest()
    {
        var graph = GraphCreator.GetParsedGraph("1 w6 : 2\n2 w5 : 1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(6));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(5));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(0));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(0));
    }
    
    [Test]
    public void OnlyEdgeWeightTest()
    {
        var graph = GraphCreator.GetParsedGraph("1 : 2 e7\n2 : 1 e1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(0));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(0));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(7));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(1));
    }
}