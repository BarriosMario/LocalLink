using System;
using System.Collections.Generic; // necesario para List<T>
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace LocalLink.Services
{
    public class PasswordService
    {
        private const int SaltSize = 16;      // 128 bits
        private const int KeySize = 32;       // 256 bits
        private const int Iterations = 100_000;

        // 🔐 Genera hash seguro con salt
        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(KeySize);

            // Guardamos: iteraciones.salt.hash
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // 🔎 Verifica contraseña ingresada contra el hash guardado
        public bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            byte[] computedHash = pbkdf2.GetBytes(KeySize);

            return CryptographicOperations.FixedTimeEquals(
                computedHash,
                storedHashBytes);
        }

        // 💪 Validación de complejidad
        public bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{10,}$";
            return Regex.IsMatch(password, pattern);
        }

        public string GetPasswordRequirementsMessage(string password)
        {
            var messages = new List<string>();

            if (string.IsNullOrEmpty(password) || password.Length < 10)
                messages.Add("mínimo 10 caracteres");

            if (!Regex.IsMatch(password, "[a-z]"))
                messages.Add("al menos una letra minúscula");

            if (!Regex.IsMatch(password, "[A-Z]"))
                messages.Add("al menos una letra mayúscula");

            if (!Regex.IsMatch(password, @"\d"))
                messages.Add("al menos un número");

            if (!Regex.IsMatch(password, @"[\W_]"))
                messages.Add("al menos un símbolo o carácter especial (%, ., _, etc.)");

            if (messages.Count == 0)
                return null; // todo correcto

            return "La contraseña debe contener: " + string.Join(", ", messages) + ".";
        }
    }
}