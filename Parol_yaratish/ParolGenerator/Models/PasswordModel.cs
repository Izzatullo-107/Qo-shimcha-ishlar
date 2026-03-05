using System.ComponentModel.DataAnnotations;

namespace ParolGenerator.Models;

public class PasswordModel
{
    [Range(4, 50, ErrorMessage = "Parol uzunligi 4 dan 50 gacha bo'lishi kerak")]
    public int Length { get; set; } = 8;

    public string GeneratedPassword { get; set; } = string.Empty;
    public bool IncludeUppercase { get; set; } = true;
    public bool IncludeLowercase { get; set; } = true;
    public bool IncludeNumbers { get; set; } = true;
    public bool IncludeSpecialChars { get; set; } = true;
    public List<int> AsciiValues { get; set; } = new();
}

public class PasswordViewModel
{
    public PasswordModel PasswordOptions { get; set; } = new();
    public List<GeneratedPassword> RecentPasswords { get; set; } = new();
}

public class GeneratedPassword
{
    public int Id { get; set; }
    public string Password { get; set; } = string.Empty;
    public int Length { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Category { get; set; }
}

public class AsciiChar
{
    public int Value { get; set; }
    public char Char { get; set; }
    public string Category { get; set; } = string.Empty;
}