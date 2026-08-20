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
                
                user.LastActive = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                var name = new Claim(ClaimTypes.Name, user.Name);
                var email = new Claim(ClaimTypes.Email, user.Email);
                var claims = new List<Claim> { name,  email };
                var identity = new ClaimsIdentity(claims,  CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authenticationProperties);
                return RedirectToAction("Index", "User");
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
                    RegisteredTime = DateTime.UtcNow,
                    ConfirmationToken = Guid.NewGuid(),
                };
                
                _dbContext.Users.Add(record);
                
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "This email is already registered.");
                    return View(model);
                }
                
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { token = record.ConfirmationToken },
                    Request.Scheme);

                await _emailService.SendEmailAsync(record.Email, confirmationLink!);
                TempData["SuccessMessage"] = "Registration Successful!";
                return RedirectToAction("Login");
            }

            [HttpGet]
            public async Task<IActionResult> ConfirmEmail(Guid token)
            {
                var user = await (
                        from u in _dbContext.Users
                        where u.ConfirmationToken == token
                        select u)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    TempData["ErrorMessage"] = "This confirmation link is invalid or has expired.";
                    return RedirectToAction("Login");
                }

                if (user.Status == AccountStatus.Blocked)
                {
                    TempData["ErrorMessage"] = "Your account has been blocked and cannot be verified.";
                    return RedirectToAction("Login");
                }

                user.Status = AccountStatus.Active;
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Verification Successful!";
                return RedirectToAction("Login");
            }

            [HttpGet]
            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }
        }