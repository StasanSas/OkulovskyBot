namespace TestProject;
using OkulovskyBot;
using System.Text.RegularExpressions;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void TestNameWeightPattern()
    {
        var request = "1 w6 : 2 e7\n2 w5 : 1 e1";
        new InputDataParser().Parse();
    }
}