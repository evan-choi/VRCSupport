using System;

static class StringExtension
{
    public static bool EqualsIgnoreCase(this string value1, string value2)
    {
        return value1.Equals(value2, StringComparison.CurrentCultureIgnoreCase);
    }
}
