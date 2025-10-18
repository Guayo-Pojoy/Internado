using System;

// Script para generar hash de contraseña BCrypt
// Ejecutar con: dotnet script GeneratePasswordHash.cs

var password = "Password123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine($"Hash as bytes length: {System.Text.Encoding.UTF8.GetBytes(hash).Length}");
