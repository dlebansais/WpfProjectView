namespace WpfProjectView.Test;

using System.Text;
using NUnit.Framework;

public class TestXamlParser
{
    [Test]
    public void TestParseEmpty()
    {
        var Content = TestTools.GetResourceContent("empty.xaml");

        Assert.Throws<InvalidXamlFormatException>(() => XamlParser.Parse(Content));
    }

    [Test]
    public void TestPrintNull()
    {
        var NullResult = XamlParser.Parse(null);
        StringBuilder BuilderResult = XamlParser.Print(NullResult);
        string PrintResult = BuilderResult.ToString();

        Assert.That(PrintResult, Is.Empty);
    }

    [Test]
    public void TestParseComplex()
    {
        string ResourceName = "complex.xaml";
        var Content = TestTools.GetResourceContent(ResourceName);

        IXamlParsingResult XamlParsingResult = XamlParser.Parse(Content);
        string ComparisonMessage = TestTools.CompareXamlParingResultWithOriginalContent(Content, XamlParsingResult);
        Assert.That(ComparisonMessage, Is.Empty, $"{ResourceName}\r\n{ComparisonMessage}");
    }
}
