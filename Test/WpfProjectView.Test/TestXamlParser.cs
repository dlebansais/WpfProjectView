namespace WpfProjectView.Test;

using NUnit.Framework;

public class TestXamlParser
{
    [Test]
    public void TestEmpty()
    {
        var Content = TestTools.GetResourceContent("empty.xaml");

        Assert.Throws<InvalidXamlFormatException>(() => XamlParser.Parse(Content));
    }
}
