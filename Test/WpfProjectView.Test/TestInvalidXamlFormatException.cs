namespace WpfProjectView.Test;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using NUnit.Framework;

public class TestInvalidXamlFormatException
{
    [Test]
    public void TestParameterlessConstructor()
    {
        var TestObject = new InvalidXamlFormatException();

#pragma warning disable CA2201 // Do not raise reserved exception types: we just need the default message, this exception will not be raised.
        var DefaultException = new Exception();
#pragma warning restore CA2201 // Do not raise reserved exception types
        string DefaultExceptionMessage = DefaultException.Message;
        string ExpectedMessage = DefaultExceptionMessage.Replace(typeof(Exception).FullName!, typeof(InvalidXamlFormatException).FullName!, StringComparison.InvariantCulture);

        Assert.That(TestObject.Message, Is.EqualTo(ExpectedMessage));
        Assert.That(TestObject.InnerException, Is.Null);
    }

    [Test]
    public void TestStringParameterConstructor()
    {
        const string TestMessage = "xyz";
        var TestObject = new InvalidXamlFormatException(TestMessage);

        Assert.That(TestObject.Message, Is.EqualTo(TestMessage));
        Assert.That(TestObject.InnerException, Is.Null);
    }
}
