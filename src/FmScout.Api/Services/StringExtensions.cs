namespace FmScout.Api.Services;

internal static class StringExtensions
{
    public static bool EndsWith(this string value, char suffix, StringComparison comparison) =>
        value.EndsWith(suffix.ToString(), comparison);
}