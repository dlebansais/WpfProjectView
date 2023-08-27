namespace WpfProjectView.Test;

using System.Diagnostics;
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

        IXamlElement? Root = XamlParsingResult.Root;
        Debug.Assert(Root is not null);

        Assert.That(Root.NameWithPrefix, Is.EqualTo("Application"));

        IXamlAttribute? ResourceAttribute = null;

        foreach (IXamlAttribute Attribute in Root.Attributes)
            if (Attribute.Name == "Resources")
            {
                ResourceAttribute = Attribute;
                break;
            }

        Assert.That(ResourceAttribute, Is.Not.Null);
    }
}
