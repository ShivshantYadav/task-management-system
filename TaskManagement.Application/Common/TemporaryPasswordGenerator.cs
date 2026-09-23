using System.Security.Cryptography;
using System.Text;

namespace TaskManagement.Application.Common
{
    /// <summary>
    /// Generates a random, human-typeable temporary password for accounts that are
    /// created on behalf of a user (by an Admin or a Manager) instead of via public
    /// self-registration. The user is expected to sign in once and change it from
    /// their Profile page.
    /// </summary>
    public static class TemporaryPasswordGenerator
    {
        private const string Uppers = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no I/O to avoid confusion
        private const string Lowers = "abcdefghijkmnopqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Symbols = "!@#$%*?";

        public static string Generate(int length = 10)
        {
            if (length < 8) length = 8;

            var all = Uppers + Lowers + Digits + Symbols;
            var chars = new char[length];

            // Guarantee at least one of each character class so it always passes
            // basic password-strength expectations.
            chars[0] = PickRandom(Uppers);
            chars[1] = PickRandom(Lowers);
            chars[2] = PickRandom(Digits);
            chars[3] = PickRandom(Symbols);

            for (var i = 4; i < length; i++)
                chars[i] = PickRandom(all);

            // Shuffle so the guaranteed characters aren't always in the same position.
            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        private static char PickRandom(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}
