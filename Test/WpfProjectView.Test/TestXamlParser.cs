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

        IXamlAttributeElementCollection? ResourceAttribute = null;

        foreach (IXamlAttribute Attribute in Root.Attributes)
            if (Attribute is IXamlAttributeElementCollection AttributeElementCollection && AttributeElementCollection.Name == "Resources")
            {
                ResourceAttribute = AttributeElementCollection;
                break;
            }

        Assert.That(ResourceAttribute, Is.Not.Null);

        IXamlElementCollection Children = ResourceAttribute.Children;

        Assert.That(Children, Has.Count.GreaterThan(0));

        foreach (IXamlElement Child in Children)
            if (Child.Namespace.Prefix.Length > 0)
                Assert.That(Child.NameWithPrefix, Does.StartWith($"{Child.Namespace.Prefix}:"));
            else
                Assert.That(Child.NameWithPrefix, Does.Not.Contain(":"));
    }
}
