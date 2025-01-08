using System;
using System.Collections.Generic;
using System.Linq;
using Konscious.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace ProyectoControlLineaBus.Clases
{
    public class Encriptado
    {
        public string Encriptar(string input)
        {
            // Generar un salt aleatorio
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Parámetros para Argon2
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(input));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4; // Número de hilos de procesamiento
            argon2.MemorySize = 65536;      // Tamaño de la memoria en KiB
            argon2.Iterations = 4;         // Número de iteraciones

            // Calcular el hash(KDF)
            byte[] hash = argon2.GetBytes(32); // 32 bytes = 256 bits

            // Concatenar el salt al hash(KDF)
            byte[] saltedHash = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, saltedHash, 0, salt.Length);
            Array.Copy(hash, 0, saltedHash, salt.Length, hash.Length);

            return Convert.ToBase64String(saltedHash);
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Decodificar el valor Base64 almacenado
            byte[] saltedHash = Convert.FromBase64String(hashedPassword);

            // Extraer el salt del valor almacenado
            byte[] salt = new byte[16];
            byte[] storedHash = new byte[32];
            Array.Copy(saltedHash, 0, salt, 0, salt.Length);
            Array.Copy(saltedHash, salt.Length, storedHash, 0, storedHash.Length);

            // Configurar Argon2 con los mismos parámetros
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4;
            argon2.MemorySize = 65536;
            argon2.Iterations = 4;

            // Calcular el hash y comparar con el hash almacenado
            byte[] hash = argon2.GetBytes(32);
            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != storedHash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}