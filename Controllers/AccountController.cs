        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Identity;
        using Task4.UserManagement.Data;
        using Task4.UserManagement.Models;
        using System.Security.Claims;
        using Microsoft.AspNetCore.Authentication;
        using Microsoft.AspNetCore.Authentication.Cookies;
        using Microsoft.EntityFrameworkCore;
        using Task4.UserManagement.Services;

        namespace Task4.UserManagement.Controllers;

        public class AccountController : Controller
        {
            public AccountController(ApplicationDbContext context, EmailService emailService)
            {
                _dbContext = context;
                _emailService = emailService;
            }
            private readonly ApplicationDbContext _dbContext;
            private readonly PasswordHasher<UsersModel>  _passwordHasher = new PasswordHasher<UsersModel>();
            private readonly EmailService _emailService;
            
            [HttpGet]
            public IActionResult Login()
            {
                LoginViewModel loginViewModel = new LoginViewModel();
                return View(loginViewModel);
            }
            
            [HttpGet]
            public IActionResult Register()
            {
                RegisterViewModel registerViewModel = new RegisterViewModel();
                return View(registerViewModel);
            }
            
            [HttpPost]
            public async Task<IActionResult> Login(LoginViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Message = "The request is invalid!";
                    return View(model);
                }

                var user = await (
                        from u in _dbContext.Users
                        where u.Email == model.Email
                        select u)
                        .FirstOrDefaultAsync();

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(model);
                }
                
                var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(model);
                }

                if (user.Status == AccountStatus.Blocked)
                {
                    ModelState.AddModelError(string.Empty, "Blocked");
                    return View(model);
                }

                var name = new Claim(ClaimTypes.Name, user.Name);
                var email = new Claim(ClaimTypes.Email, user.Email);
                var claims = new List<Claim> { name,  email };
                var identity = new ClaimsIdentity(claims,  CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }
            
            [HttpPost]
            public async Task<IActionResult> Register(RegisterViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Message = "The request is invalid!";
                    return View(model);
                }

                var newUser = await (
                        from u in _dbContext.Users
                        where u.Email == model.Email
                        select u)
                    .FirstOrDefaultAsync();

                if (newUser != null)
                {
                    ModelState.AddModelError(string.Empty, "This email is already registered.");
                    return View(model);
                }

                var hashedPassword = _passwordHasher.HashPassword(null, model.Password);
                var record = new UsersModel()
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    Status = AccountStatus.Unverified,
                    LastActive = DateTime.UtcNow,
                    RegisteredTime = DateTime.UtcNow
                };
                
                _dbContext.Users.Add(record);
                await _dbContext.SaveChangesAsync();
                _ = _emailService.SendEmailAsync(record.Email, "https://localhost:5201/Account/ConfirmEmail?email=" + record.Email);
                TempData["SuccessMessage"] = "Registration Successful!";
                return RedirectToAction("Login");
            }
        }