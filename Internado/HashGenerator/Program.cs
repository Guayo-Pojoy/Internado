using System;

namespace HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var password = "Password123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            Console.WriteLine("===========================================");
            Console.WriteLine("Generador de Hash para Base de Datos");
            Console.WriteLine("===========================================");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash BCrypt: {hash}");
            Console.WriteLine();

            // Convertir a formato hexadecimal para SQL
            var bytes = System.Text.Encoding.UTF8.GetBytes(hash);
            var hex = "0x" + BitConverter.ToString(bytes).Replace("-", "");

            Console.WriteLine($"Hash en Hex (para SQL): {hex}");
            Console.WriteLine($"Length: {bytes.Length} bytes");
            Console.WriteLine();
            Console.WriteLine("Copiar el valor Hex para usar en el script SQL");
            Console.WriteLine("===========================================");
        }
    }
}
