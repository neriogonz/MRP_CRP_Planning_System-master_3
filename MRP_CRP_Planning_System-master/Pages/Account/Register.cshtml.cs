using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MetallurgyAnalytics.Data;
using MetallurgyAnalytics.Models;
using MetallurgyAnalytics.Services;

namespace MetallurgyAnalytics.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;

        public RegisterModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [StringLength(255, ErrorMessage = "ФИО не должно превышать 255 символов")]
            public string? FullName { get; set; }

            [Required(ErrorMessage = "Логин обязателен")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 100 символов")]
            [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Логин может содержать только латинские буквы, цифры и подчеркивание")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Некорректный формат Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Пароль обязателен")]
            [StringLength(255, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Подтвердите пароль")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Пароли не совпадают");
                return Page();
            }

            if (!ModelState.IsValid) return Page();

            var userExists = await _context.Users.AnyAsync(u => u.Username == Input.Username);
            if (userExists)
            {
                ModelState.AddModelError("Input.Username", "Пользователь с таким логином уже существует");
                return Page();
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == Input.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Input.Email", "Этот Email уже зарегистрирован");
                return Page();
            }

            var user = new User
            {
                Username = Input.Username,
                Email = Input.Email,
                FullName = Input.FullName,
                PasswordHash = AuthService.HashPassword(Input.Password),
                Role = "user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"✅ Пользователь {user.Username} успешно зарегистрирован! Теперь войдите.";
            return RedirectToPage("Login");
        }
    }
}