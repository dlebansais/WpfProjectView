﻿namespace WpfProjectView.Test;

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using NUnit.Framework;

[TestFixture]
public class TestXamlParser
{
    [Test]
    public void TestParseEmpty1()
    {
        var Content = TestTools.GetResourceContent("empty.xaml");

        _ = Assert.Throws<InvalidXamlFormatException>(() => XamlParser.Parse(Content));
    }

    [Test]
    public async Task TestParseEmpty2()
    {
        (FolderView.ILocation Location, FolderView.IPath PathToProject) = TestTools.GetLocalLocationAndPathToProject();

        IProject TestProject = await Project.CreateAsync(Location, PathToProject).ConfigureAwait(false);

        foreach (IFile Item in TestProject.Files)
        {
            if (Item.Path.Name == "Empty.xaml")
            {
                Item.Parse();

                Assert.That(!Item.IsParsed);
            }
        }
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
    public void TestPrintInvalidResult()
    {
        _ = Assert.Throws<ArgumentNullException>(() => XamlParser.Print(null!));
    }

    [Test]
    public void TestParseComplex()
    {
        const string ResourceName = "complex.xaml";
        var Content = TestTools.GetResourceContent(ResourceName);

        IXamlParsingResult XamlParsingResult = XamlParser.Parse(Content);
        string ComparisonMessage = TestTools.CompareXamlParingResultWithOriginalContent(Content, XamlParsingResult);

        Assert.That(ComparisonMessage, Is.Empty, $"{ResourceName}\r\n{ComparisonMessage}");

        IXamlElement Root = Contract.AssertNotNull(XamlParsingResult.Root);

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

    [Test]
    [TestCase("single1.xaml")]
    [TestCase("single2.xaml")]
    [TestCase("single3.xaml")]
    [TestCase("single4.xaml")]
    [TestCase("single5.xaml")]
    public void TestParseSingle(string resourceName)
    {
        var Content = TestTools.GetResourceContent(resourceName);

        IXamlParsingResult XamlParsingResult = XamlParser.Parse(Content);
        string ComparisonMessage = TestTools.CompareXamlParingResultWithOriginalContent(Content, XamlParsingResult);

        Assert.That(ComparisonMessage, Is.Empty, $"{resourceName}\r\n{ComparisonMessage}");
    }
}
