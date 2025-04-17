using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace WebApplication1.HelperClasses
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Հեշավորում ենք
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            // Վերադարձնում ենք salt + hashed
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public static bool VerifyPassword(string storedPassword, string enteredPassword)
        {
            var parts = storedPassword.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            // Հաշվում ենք մուտքագրված գաղտնաբառի հեշը
            string enteredHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hash == enteredHash;
        }
    }
}
