// Internado.Infrastructure/Security/BCryptPasswordHasher.cs
using System;
using System.Text;

namespace Internado.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        // Requerido por la interfaz
        public string HashToBase64(string plainPassword, int workFactor)
        {
            // Genera el hash de BCrypt en TEXTO ($2b$...) con el workFactor indicado
            var hashText = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor);
            // Lo guardamos como Base64 de los bytes UTF8 del hash de texto
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(hashText));
        }

        // Overload opcional por conveniencia (workFactor por defecto = 10)
        public string HashToBase64(string plainPassword) => HashToBase64(plainPassword, 10);

        // Verificación tolerante: acepta Base64 del hash o el hash de texto directamente
        public bool VerifyFromBase64(string plainPassword, string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return false;

            string hashText;
            try
            {
                // Si 'stored' es Base64, lo decodificamos…
                var bytes = Convert.FromBase64String(stored);
                hashText = Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                // …si no, asumimos que ya es el hash BCrypt en texto ($2a$/$2b$)
                hashText = stored;
            }

            return BCrypt.Net.BCrypt.Verify(plainPassword, hashText);
        }
    }
}
