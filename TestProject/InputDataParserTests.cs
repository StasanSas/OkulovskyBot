namespace TestProject;
using Graph.App;
using Graph.Dom.Algoritms;

public class Tests
{
    private enum TestEnum
    {
        
    }
    
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void FullNameWeightPatternTest()
    {
        var graph = GraphCreator.GetParsedGraph<TestEnum>("1 w6 : 2 e7\n2 w5 : 1 e1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(6));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(5));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(7));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(1));
    }
    
    [Test]
    public void OnlyNodeWeightTest()
    {
        var graph = GraphCreator.GetParsedGraph<TestEnum>("1 w6 : 2\n2 w5 : 1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(6));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(5));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(0));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(0));
    }
    
    [Test]
    public void OnlyEdgeWeightTest()
    {
        var graph = GraphCreator.GetParsedGraph<TestEnum>("1 : 2 e7\n2 : 1 e1");
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(0));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(0));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(7));
        Assert.That(graph.Edges["2", "1"].Weight, Is.EqualTo(1));
    }
    
    [Test]
    public void SimpleCreationTest()
    {
        var graph = GraphCreator.GetParsedGraph<TestEnum>(" 1 : 2   \n2 :");
        Assert.That(graph.Nodes.Count(), Is.EqualTo(2));
        Assert.That(graph.Nodes["1"].Weight, Is.EqualTo(0));
        Assert.That(graph.Nodes["2"].Weight, Is.EqualTo(0));
        Assert.That(graph.Edges["1", "2"].Weight, Is.EqualTo(0));
    }
    
    [Test]
    public void OKMemoryLimitTest()
    {
        // 1 запуск
        for (var i = 0; i < 1; i++)
        {
            var graph = GraphCreator.GetParsedGraph<N>("1 : 2 e6 3 e7 4 e8\n2 : 1 e6 3 e3 4 e5\n3 : 1 e7 2 e6 4 e8\n4 : 1 e6 2 e7 3 e6",
                true);
            var changes = Dijkstra.GetObserverForGraph(graph,
                graph.Nodes.First().Id,graph.Nodes.Last().Id);
            var vis = new Visualizator<string, int, N>(changes, Dijkstra.DefineColor);
            // нужно поменять путь()()
            System.IO.File.Create(@"C:\Users\haier\Desktop\prog\OkulovskyBot\Gifs\1134467270.gif").Close();
            vis.StartVisualize(25, @"C:\Users\haier\Desktop\prog\OkulovskyBot\Gifs\1134467270.gif");
        }
    }
    
    [Test]
    public void NOTOKMemoryLimitTest()
    {
        // 2 раза запуск
        for (var i = 0; i < 2; i++)
        {
            var graph = GraphCreator.GetParsedGraph<N>("1 : 2 e6 3 e7 4 e8\n2 : 1 e6 3 e3 4 e5\n3 : 1 e7 2 e6 4 e8\n4 : 1 e6 2 e7 3 e6",
                true);
            var changes = Dijkstra.GetObserverForGraph(graph,
                graph.Nodes.First().Id,graph.Nodes.Last().Id);
            var vis = new Visualizator<string, int, N>(changes, Dijkstra.DefineColor);
            // нужно поменять путь()()
            System.IO.File.Create(@"C:\Users\haier\Desktop\prog\OkulovskyBot\Gifs\1134467270.gif").Close();
            vis.StartVisualize(25, @"C:\Users\haier\Desktop\prog\OkulovskyBot\Gifs\1134467270.gif");
        }
    }
}