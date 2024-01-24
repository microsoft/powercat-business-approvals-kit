using System.Text;
using Xunit.Abstractions;

namespace test;

public class ScannerTests
{
    public ScannerTests(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
    }

    #region constants
    private static string YAML_TEMPLATE = @"""scrWorkflowDesignerScreen As screen.'autoLayout_HeaderFooter_ver1.0'"":
    MainContainer As groupContainer.verticalAutoLayoutContainer:
        Test:
            {0}
    ";

    private static string IGNORE_PROPERTIES_TEMPLATE = @"IgnoreProperties:
  - {0}";

    private static string IGNORE_FUNCTIONS_TEMPLATE = @"IgnoreFunctions:
  - {0}";

    private static string EMPTY = string.Format(YAML_TEMPLATE,"");
    #endregion

    [Theory]
    [MemberData(nameof(NoResultTestCases))]
    public void NoResults(string yaml)
    {
        // Arrange
        var scanner = new Scanner();
        byte[] byteArray = Encoding.ASCII.GetBytes( yaml );
        MemoryStream stream = new MemoryStream( byteArray );

        // Act
        var results = scanner.Scan(stream);

        // Assert
        Assert.Empty(results);
    }

    public static IEnumerable<object[]> NoResultTestCases =>
    new List<object[]>
    {
        new object[] { "" },
        new object[] { EMPTY },
        // Shoould be ignored as it is referencing a non scalar string
        new object[] { string.Format(YAML_TEMPLATE, "Text: =Other.Value")},
        // Should be ignored as blank value
        new object[] { string.Format(YAML_TEMPLATE, "Text: =\"\"")},
        // Should be ignored as empty string
        new object[] { string.Format(YAML_TEMPLATE, "Text: =\" \"")},
        // Should be ignored as numeric
        new object[] { string.Format(YAML_TEMPLATE, "ContentHeight: = \"500\"")},
        // Ignore numeric of objects
        new object[] { string.Format(YAML_TEMPLATE, "UpdateContext({  contentHeight: 500 })") },
        // Should be ignored as table name
        new object[] { string.Format(YAML_TEMPLATE, "OnVisible: =Refresh('Some Table')")}
        
    };

    [Theory]
    [MemberData(nameof(IgnoreColorsTestCases))]
    [MemberData(nameof(IgnoreRecordTestCases))]
    [MemberData(nameof(IgnoreStringLiteralsTestCases))]
    public void NoResultsWithConfig(string yaml, string config)
    {
        // Arrange
        var scanner = new Scanner();
        byte[] byteArray = Encoding.ASCII.GetBytes( yaml );
        MemoryStream stream = new MemoryStream( byteArray );

        byte[] configByteArray = Encoding.ASCII.GetBytes( config );
        MemoryStream configStream = new MemoryStream( configByteArray );

        // Act
        var results = scanner.Scan(stream, configStream);

        // Assert
        Assert.Empty(results);
    }

    public static IEnumerable<object[]> IgnoreColorsTestCases =>
    new List<object[]>
    {
        // Should be ignored as it a color
        new object[] { string.Format(YAML_TEMPLATE, "Color: = ColorValue(\"#ffffff\")"), "Settings:\r\n  IgnoreColors: True"},
        // Should be ignored as it a three character color
        new object[] { string.Format(YAML_TEMPLATE, "Color: = ColorValue(\"#FFF\")"), "Settings:\r\n  IgnoreColors: True"}
    };

    public static IEnumerable<object[]> IgnoreRecordTestCases =>
    new List<object[]>
    {
        // Ignore variable with non text value
        new object[] { "Test:\r\n  OnVisible: = Set(Constant, 1)", "IgnoreVariableSetup:\r\n  - Constant"},
        // Ignore string record value
        new object[] { "Test:\r\n  OnVisible: |-\r\n    = Set(Constant, {a: \"value\"})", "IgnoreVariableSetup:\r\n  - Constant"},
        // Ignore number record value
        new object[] { "Test:\r\n  OnVisible: |-\r\n    = Set(Constant, {a: 1)", "IgnoreVariableSetup:\r\n  - Constant"}
    };

    public static IEnumerable<object[]> IgnoreStringLiteralsTestCases =>
    new List<object[]>
    {
        // Ignore variable with non text value
        new object[] { "Test:\r\n  ItemKey: = \"Value\"", "IgnoreValues:\r\n  - Value"}
    };
    
    [Theory]
    [MemberData(nameof(IgnoreTestCases))]
    public void NoForIgnoreResults(string yaml, string ignore)
    {
        // Arrange
        var scanner = new Scanner();

        byte[] byteArray = Encoding.ASCII.GetBytes( yaml );
        MemoryStream stream = new MemoryStream( byteArray );

        byteArray = Encoding.ASCII.GetBytes( ignore );
        MemoryStream configStream = new MemoryStream( byteArray );

        // Act
        var results = scanner.Scan(stream, configStream);

        // Assert
        Assert.Empty(results);
    }

    public static IEnumerable<object[]> IgnoreTestCases =>
    new List<object[]>
    {
        // Should be ignored as ignore property is configured
        new object[] { 
            string.Format(YAML_TEMPLATE, "ItemKey: = \"value\""),
            string.Format(IGNORE_PROPERTIES_TEMPLATE, "ItemKey")
        },
        // Should be ignored as ignore property is configured
        new object[] { 
            string.Format(YAML_TEMPLATE, @"Items: |-
                = Table(
                    { ItemKey: ""value"" }
                   )"),
            string.Format(IGNORE_PROPERTIES_TEMPLATE, "ItemKey")
        },
        // Ignore Function
        new object[] { 
            string.Format(YAML_TEMPLATE, "ItemKey: = Foo(\"value\")"),
            string.Format(IGNORE_FUNCTIONS_TEMPLATE, "Foo")
        }
    };

    [Theory]
    [MemberData(nameof(SingleResultTestCases))]
    public void SingleResult(string yaml)
    {
        // Arrange
        var scanner = new Scanner();
        byte[] byteArray = Encoding.ASCII.GetBytes( yaml );
        MemoryStream stream = new MemoryStream( byteArray );

        // Act
        var results = scanner.Scan(stream);

        // Assert
        Assert.Single(results);
    }

    public static IEnumerable<object[]> SingleResultTestCases =>
    new List<object[]>
    {
        // Simple case, the text is only a literal
        new object[] { string.Format(YAML_TEMPLATE, "Text: =\"Other\"") },
        // The expression contains a literal as part of an expression
        new object[] { string.Format(YAML_TEMPLATE, "Text: =\"Other\" & \" \" & \"More\"")},
        // The expression contains a literal as part of an function parameter
        new object[] { string.Format(YAML_TEMPLATE, "Text: =If(1=2,\"How?\", \"No\")")}
    };

    private class Converter : TextWriter
    {
        ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }

        public override void Write(char value)
        {
            
        }
    }
}