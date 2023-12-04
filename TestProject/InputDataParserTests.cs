namespace TestProject;
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
        string pattern = @"^(.)+ [Ww]-?\d+&";
        Regex regex = new Regex(pattern);
        string input = "John W-150&";
        
        Assert.True(regex.IsMatch(input));
    }
}