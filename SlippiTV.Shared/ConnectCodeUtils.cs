using System.Text.RegularExpressions;

namespace SlippiTV.Shared;

public static partial class ConnectCodeUtils
{
    [GeneratedRegex(@"^[A-Za-z]+#[0-9]+$")]
    private static partial Regex _connectCodeRegex();
    public static bool IsValidConnectCode(string code)
    {
        return _connectCodeRegex().IsMatch(code);
    }

    public static string NormalizeConnectCode(string code) => code.ToUpper();
    public static string SanitizeConnectCode(string code) => code.Replace("#", "-");
    public static string UnsanitizeConnectCode(string code) => code.Replace("-", "#");
}
