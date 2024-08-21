namespace Fake2;

public class ContentPropertyAttribute
{
    public ContentPropertyAttribute(int dummy)
    {
        Dummy = dummy;
    }

    public int Dummy { get; }
}
