using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4.UserManagement.Data;
using Task4.UserManagement.Models;

namespace Task4.UserManagement.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    public UserController(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _dbContext.Users.OrderByDescending(u => u.LastActive).ToListAsync();
        return View(users);
    }
    
    [HttpPost]
    public async Task<IActionResult> Block([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return BadRequest(new { message = "Please select at least one user." });
        }
        
        var usersToBlock = await _dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        if (usersToBlock.Count == 0)
        {
            return NotFound(new { message = "Selected users were not found." });
        }
        
        foreach (var user in usersToBlock)
        {
            user.Status = AccountStatus.Blocked;
        }
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Users blocked successfully." });
    }
    
    [HttpPost]
    public async Task<IActionResult> Unblock([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return BadRequest(new { message = "Please select at least one user." });
        }
        
        var users = await _dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        if (users.Count == 0)
        {
            return NotFound(new { message = "Selected users were not found." });
        }
        
        foreach (var user in users)
        {
            user.Status = AccountStatus.Active;
        }
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Users unblocked successfully." });
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return BadRequest(new { message = "Please select at least one user." });
        }
        
        var users = await _dbContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        if (users.Count == 0)
        {
            return NotFound(new { message = "Selected users were not found." });
        }
        
        _dbContext.Users.RemoveRange(users);
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Users deleted successfully." });
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteUnverified()
    {
        var users = await _dbContext.Users.Where(u => u.Status == AccountStatus.Unverified).ToListAsync();
        _dbContext.Users.RemoveRange(users);
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "All unverified users deleted successfully." });
    }
}