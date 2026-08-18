using Microsoft.AspNetCore.Mvc;
using Task4.UserManagement.Models;

namespace Task4.UserManagement.Controllers;

public class AccountController : Controller
{
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
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Message = "The request is invalid!";
            return View(model);
        }
        
        //TODO: check credentials
        return Empty;
    }
}