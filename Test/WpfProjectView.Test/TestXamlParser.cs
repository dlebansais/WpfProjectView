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
}
