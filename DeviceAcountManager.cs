using System.Text.Json;
using Microsoft.Maui.Storage; // Preferences

namespace LocalLink.Services 
{
    public class DeviceAccountManager
    {
        private const string Key = "DeviceAccounts";
        private const int MaxAccounts = 3;

        // Guardar una cuenta (email)
        public static bool TryAddAccount(string email)
        {
            var accounts = GetAccounts();

            // Si ya existe la cuenta, no agregar duplicado
            if (accounts.Contains(email, StringComparer.OrdinalIgnoreCase))
                return true;

            if (accounts.Count >= MaxAccounts)
                return false; // límite alcanzado

            accounts.Add(email);
            SaveAccounts(accounts);
            return true;
        }

        public static List<string> GetAccounts()
        {
            string json = Preferences.Get(Key, null);
            if (string.IsNullOrEmpty(json))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        private static void SaveAccounts(List<string> accounts)
        {
            string json = JsonSerializer.Serialize(accounts);
            Preferences.Set(Key, json);
        }

        public static void RemoveAccount(string email)
        {
            var accounts = GetAccounts();

            accounts.RemoveAll(a =>
                a.Equals(email, StringComparison.OrdinalIgnoreCase));

            SaveAccounts(accounts);
        }

        public static bool HasAccounts()
        {
            return GetAccounts().Count > 0;
        }

    }
}