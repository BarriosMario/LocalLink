using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LocalLink.Services;
using LocalLink.Models;

namespace LocalLink.Repositories
{
    public class UserRepository
    {
        private readonly EncryptionService _encryptionService;
        private readonly string _filePath;

        public UserRepository()
        {
            _encryptionService = new EncryptionService();
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "userData.json");
        }

        public async Task<List<UserData>> GetAllUsersAsync()
        {
            if (!File.Exists(_filePath))
                return new List<UserData>();

            byte[] key = await _encryptionService.GetOrCreateEncryptionKey();
            byte[] encryptedData = File.ReadAllBytes(_filePath);

            string decryptedJson = _encryptionService.Decrypt(encryptedData, key);

            if (string.IsNullOrWhiteSpace(decryptedJson))
                return new List<UserData>();

            return JsonSerializer.Deserialize<List<UserData>>(decryptedJson)
                   ?? new List<UserData>();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var users = await GetAllUsersAsync();
            return users.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var users = await GetAllUsersAsync();
            return users.Any(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task AddUserAsync(UserData user)
        {
            var users = await GetAllUsersAsync();

            users.Add(user);

            string json = JsonSerializer.Serialize(users);

            byte[] key = await _encryptionService.GetOrCreateEncryptionKey();
            byte[] encryptedData = _encryptionService.Encrypt(json, key);

            File.WriteAllBytes(_filePath, encryptedData);
        }

        public async Task UpdateUserAsync(UserData updatedUser)
        {
            var users = await GetAllUsersAsync();
            int index = users.FindIndex(u => u.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                users[index] = updatedUser;
                string json = JsonSerializer.Serialize(users);
                byte[] key = await _encryptionService.GetOrCreateEncryptionKey();
                byte[] encryptedData = _encryptionService.Encrypt(json, key);
                File.WriteAllBytes(_filePath, encryptedData);
            }
        }

        public async Task DeleteUserAsync(string email)
        {
            var users = await GetAllUsersAsync();
            users.RemoveAll(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            string json = JsonSerializer.Serialize(users);

            byte[] key = await _encryptionService.GetOrCreateEncryptionKey();
            byte[] encryptedData = _encryptionService.Encrypt(json, key);

            File.WriteAllBytes(_filePath, encryptedData);
        }
    }
}