using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Moq;

namespace test;

public class AnalyticsTests
{
    [Theory]
    [MemberData(nameof(TestCases))]
    public void ExpectedResults(List<ScanResult> input, string output)
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var data = new MemoryStream();

        // Act
        var analytics = new Analytics(logger.Object);
        analytics.Generate(input, data);

        data.Position = 0;

        var result = new StreamReader(data).ReadToEnd(); 

        // Assert
        
        var expected = ReadAllLines(new StringReader(output));
        var actual = ReadAllLines(new StringReader(result));
        Assert.Equal(expected.Count, actual.Count);
        for ( int i = 0; i < expected.Count; i++ ) {
            if ( expected[i].StartsWith("*;") ) {
                var start = actual[i].IndexOf(";");
                Assert.Equal(expected[i].Substring(2), actual[i].Substring(start+1) );
            } else {
                Assert.Equal(expected[i], actual[i]);
            }
        }
        
    }

    private List<string> ReadAllLines(StringReader reader) {
        List<string> items = new List<string>();

        var line = "";
        while ((line = reader.ReadLine()) != null) {
            items.Add(line);
        }

        return items;
    }

    static string HEADER = "when;control;parent;property;text\r\n";

    public static IEnumerable<object[]> TestCases =>
    new List<object[]>
    {
        // Empty file
        new object[] { new List<ScanResult>(), HEADER },

        // No parent
        new object[] { new List<ScanResult>() { new ScanResult { Control = "1", Property = "2", Text = new List<string> {"3"} }}, HEADER + "*;1;;2;\"3\"\r\n" },

        // Single parent
        new object[] { new List<ScanResult>() { new ScanResult { Control = "1.a", Property = "2", Text = new List<string> {"3"} }}, HEADER + "*;1.a;a;2;\"3\"\r\n" },

        // Multiple parent
        new object[] { new List<ScanResult>() { new ScanResult { Control = "1.a.b", Property = "2", Text = new List<string> {"3"} }}, HEADER + "*;1.a.b;b;2;\"3\"\r\n" },
        
        // Quoted parent
        new object[] { new List<ScanResult>() { new ScanResult { Control = "\"Some text\".a", Property = "2", Text = new List<string> {"3"} }}, HEADER + "*;\"Some text\".a;a;2;\"3\"\r\n" },

        // Quoted parent with delimiter
        new object[] { new List<ScanResult>() { new ScanResult { Control = "\"Some.text\".a", Property = "2", Text = new List<string> {"3"} }}, HEADER + "*;\"Some.text\".a;a;2;\"3\"\r\n" }
    };
}