using System.Security.Cryptography;


namespace LocalLink.Services
{
    public class EncryptionService
    {
        //llave de encriptación en base64
        public async Task<byte[]> GetOrCreateEncryptionKey()
        {
            string storedKey = await SecureStorage.Default.GetAsync("encryption_key");

            if (string.IsNullOrEmpty(storedKey))
            {
                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.GenerateKey();

                string keyString = Convert.ToBase64String(aes.Key);
                await SecureStorage.Default.SetAsync("encryption_key", keyString);

                return aes.Key;
            }

            return Convert.FromBase64String(storedKey);
        }

        //Se encrypta
        public byte[] Encrypt(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var ms = new MemoryStream();

            // Guardamos el IV primero
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return ms.ToArray();
        }

        //se desencripta
        public string Decrypt(byte[] cipherText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;

            using var ms = new MemoryStream(cipherText);
            byte[] iv = new byte[16];
            ms.Read(iv, 0, iv.Length);

            aes.IV = iv;

            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}