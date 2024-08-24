#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace PgCompletionist;

using System.Windows;
using System.Windows.Controls;

public class TestGrid : Grid
{
    public static readonly DependencyProperty TestProperty = DependencyProperty.Register(nameof(TestProperty), typeof(string), typeof(TestGrid));

    public static void SetTest(DependencyObject obj, string value)
    {
    }

    public static string GetTest(DependencyObject obj)
    {
        return string.Empty;
    }
}
