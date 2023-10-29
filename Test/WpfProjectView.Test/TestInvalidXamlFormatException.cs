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

        var DefaultException = new Exception();
        string DefaultExceptionMessage = DefaultException.Message;
        string ExpectedMessage = DefaultExceptionMessage.Replace(typeof(Exception).FullName!, typeof(InvalidXamlFormatException).FullName!);

        Assert.That(TestObject.Message, Is.EqualTo(ExpectedMessage));
        Assert.That(TestObject.InnerException, Is.Null);
    }

    [Test]
    public void TestStringParameterConstructor()
    {
        string TestMessage = "xyz";
        var TestObject = new InvalidXamlFormatException(TestMessage);

        Assert.That(TestObject.Message, Is.EqualTo(TestMessage));
        Assert.That(TestObject.InnerException, Is.Null);
    }

    [Test]
    public void TestSerializationConstructor()
    {
        string InnerExceptionMessage = "InnerTest";
        var InnerException = new Exception(InnerExceptionMessage);
        string TestMessage = "Test";
        var TestObject = new InvalidXamlFormatException(TestMessage, InnerException);

        MemoryStream stream = new();
        SoapFormatter formatter = new(null, new StreamingContext(StreamingContextStates.File));
        formatter.Serialize(stream, TestObject);

        stream.Position = 0;

        var DeserializedObject = (InvalidXamlFormatException)formatter.Deserialize(stream);

        Assert.That(DeserializedObject, Is.Not.Null);
        Assert.That(DeserializedObject?.Message, Is.EqualTo(TestMessage));

        Exception? DeserializeInnerException = DeserializedObject?.InnerException;

        Assert.That(DeserializeInnerException, Is.Not.Null);
        Assert.That(DeserializeInnerException?.Message, Is.EqualTo(InnerExceptionMessage));
    }
}
