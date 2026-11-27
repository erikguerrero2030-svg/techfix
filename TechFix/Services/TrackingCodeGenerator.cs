using System.Security.Cryptography;

namespace TechFix.Services;
public static class TrackingCodeGenerator
{
    public static string Create() => Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
}
