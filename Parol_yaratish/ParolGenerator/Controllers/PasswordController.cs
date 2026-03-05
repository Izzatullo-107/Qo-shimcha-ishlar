using Microsoft.AspNetCore.Mvc;
using ParolGenerator.Models;
using System.Text;

namespace ParolGenerator.Controllers;

public class PasswordController : Controller
{
    private static List<GeneratedPassword> _recentPasswords = new();

    public IActionResult Index()
    {
        var viewModel = new PasswordViewModel
        {
            PasswordOptions = new PasswordModel(),
            RecentPasswords = _recentPasswords.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Generate(PasswordModel model)
    {
        // Model state ni tekshirish
        if (!ModelState.IsValid)
        {
            var errorViewModel = new PasswordViewModel
            {
                PasswordOptions = model,
                RecentPasswords = _recentPasswords.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
            };
            return View("Index", errorViewModel);
        }

        // Tanlangan belgi turlarini tekshirish
        if (!model.IncludeUppercase && !model.IncludeLowercase && !model.IncludeNumbers && !model.IncludeSpecialChars)
        {
            ModelState.AddModelError("", "Kamida bitta belgi turini tanlang!");

            var errorViewModel = new PasswordViewModel
            {
                PasswordOptions = model,
                RecentPasswords = _recentPasswords.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
            };
            return View("Index", errorViewModel);
        }

        // Ruxsat etilgan ASCII qiymatlarini yig'ish
        var allowedAscii = new List<int>();

        if (model.IncludeNumbers)
            allowedAscii.AddRange(Enumerable.Range(48, 10));  // 0-9 (48-57)

        if (model.IncludeUppercase)
            allowedAscii.AddRange(Enumerable.Range(65, 26));  // A-Z (65-90)

        if (model.IncludeLowercase)
            allowedAscii.AddRange(Enumerable.Range(97, 26));  // a-z (97-122)

        if (model.IncludeSpecialChars)
        {
            allowedAscii.AddRange(Enumerable.Range(33, 15));   // !"#$%&'()*+,-./
            allowedAscii.AddRange(Enumerable.Range(58, 7));    // :;<=>?@
            allowedAscii.AddRange(Enumerable.Range(91, 6));    // [\]^_`
            allowedAscii.AddRange(Enumerable.Range(123, 4));   // {|}~
        }

        var random = new Random();
        var passwordBuilder = new StringBuilder();
        var asciiValues = new List<int>();

        // Model.Length bo'yicha parol yaratish
        for (int i = 0; i < model.Length; i++)
        {
            int asciiValue = allowedAscii[random.Next(allowedAscii.Count)];
            asciiValues.Add(asciiValue);
            passwordBuilder.Append((char)asciiValue);
        }

        model.GeneratedPassword = passwordBuilder.ToString();
        model.AsciiValues = asciiValues;

        // Saqlash
        _recentPasswords.Add(new GeneratedPassword
        {
            Id = _recentPasswords.Count + 1,
            Password = model.GeneratedPassword,
            Length = model.Length,
            CreatedAt = DateTime.Now,
            Category = GetPasswordCategory(model)
        });

        if (_recentPasswords.Count > 20)
            _recentPasswords = _recentPasswords.Skip(_recentPasswords.Count - 20).ToList();

        var viewModel = new PasswordViewModel
        {
            PasswordOptions = model,
            RecentPasswords = _recentPasswords.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
        };

        return View("Index", viewModel);
    }

    public IActionResult AsciiTable()
    {
        var asciiTable = new List<AsciiChar>();

        for (int i = 32; i <= 126; i++)
        {
            asciiTable.Add(new AsciiChar
            {
                Value = i,
                Char = (char)i,
                Category = GetAsciiCategory(i)
            });
        }

        return View(asciiTable);
    }

    [HttpGet]
    public IActionResult CheckStrength(string password)
    {
        int score = 0;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score++;

        string strength = score switch
        {
            <= 2 => "Zaif",
            <= 4 => "O'rtacha",
            <= 5 => "Kuchli",
            >= 6 => "Juda kuchli"
        };

        return Json(new { Score = score, Strength = strength });
    }

    private string GetAsciiCategory(int asciiValue)
    {
        if (asciiValue >= 48 && asciiValue <= 57) return "Raqam";
        if (asciiValue >= 65 && asciiValue <= 90) return "Katta harf";
        if (asciiValue >= 97 && asciiValue <= 122) return "Kichik harf";
        return "Maxsus belgi";
    }

    private string GetPasswordCategory(PasswordModel model)
    {
        var categories = new List<string>();
        if (model.IncludeUppercase) categories.Add("Katta");
        if (model.IncludeLowercase) categories.Add("Kichik");
        if (model.IncludeNumbers) categories.Add("Raqam");
        if (model.IncludeSpecialChars) categories.Add("Maxsus");

        return string.Join("+", categories);
    }

    // Talab qilingan formatda parol generatsiya qilish
    [HttpPost]
    public IActionResult GenerateFormattedPassword(PasswordModel model)
    {
        // Format: 2 raqam, 1 belgi, 3 kichik harf, 2 katta harf
        var random = new Random();
        var passwordChars = new List<char>();
        var asciiValues = new List<int>();

        // 2 ta raqam (48-57)
        for (int i = 0; i < 2; i++)
        {
            int ascii = random.Next(48, 58);
            passwordChars.Add((char)ascii);
            asciiValues.Add(ascii);
        }

        // 1 ta maxsus belgi
        int specialAscii = GetRandomSpecialChar(random);
        passwordChars.Add((char)specialAscii);
        asciiValues.Add(specialAscii);

        // 3 ta kichik harf (97-122)
        for (int i = 0; i < 3; i++)
        {
            int ascii = random.Next(97, 123);
            passwordChars.Add((char)ascii);
            asciiValues.Add(ascii);
        }

        // 2 ta katta harf (65-90)
        for (int i = 0; i < 2; i++)
        {
            int ascii = random.Next(65, 91);
            passwordChars.Add((char)ascii);
            asciiValues.Add(ascii);
        }

        // Belgilarni aralashtirish
        var shuffled = passwordChars.Select((c, i) => new { Char = c, Index = i })
                                    .OrderBy(x => random.Next())
                                    .ToList();

        var shuffledAscii = asciiValues.Select((a, i) => new { Value = a, Index = i })
                                       .OrderBy(x => random.Next())
                                       .Select(x => x.Value)
                                       .ToList();

        string password = new string(shuffled.Select(x => x.Char).ToArray());

        model.GeneratedPassword = password;
        model.AsciiValues = shuffledAscii;
        model.Length = 8; // Formatlangan parol har doim 8 belgi
        model.IncludeUppercase = true;
        model.IncludeLowercase = true;
        model.IncludeNumbers = true;
        model.IncludeSpecialChars = true;

        // Saqlash
        _recentPasswords.Add(new GeneratedPassword
        {
            Id = _recentPasswords.Count + 1,
            Password = password,
            Length = 8,
            CreatedAt = DateTime.Now,
            Category = "Formatlangan (2R+1B+3K+2K)"
        });

        if (_recentPasswords.Count > 20)
            _recentPasswords = _recentPasswords.Skip(_recentPasswords.Count - 20).ToList();

        var viewModel = new PasswordViewModel
        {
            PasswordOptions = model,
            RecentPasswords = _recentPasswords.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
        };

        return View("Index", viewModel);
    }

    // Yordamchi metodlar
    private int GetRandomSpecialChar(Random random)
    {
        var specialRanges = new List<(int start, int end)>
    {
        (33, 47),   // ! " # $ % & ' ( ) * + , - . /
        (58, 64),   // : ; < = > ? @
        (91, 96),   // [ \ ] ^ _ `
        (123, 126)  // { | } ~
    };

        var range = specialRanges[random.Next(specialRanges.Count)];
        return random.Next(range.start, range.end + 1);
    }

    private List<int> GetAllSpecialChars()
    {
        var specials = new List<int>();
        specials.AddRange(Enumerable.Range(33, 15));  // ! " # $ % & ' ( ) * + , - . /
        specials.AddRange(Enumerable.Range(58, 7));   // : ; < = > ? @
        specials.AddRange(Enumerable.Range(91, 6));   // [ \ ] ^ _ `
        specials.AddRange(Enumerable.Range(123, 4));  // { | } ~
        return specials;
    }

}