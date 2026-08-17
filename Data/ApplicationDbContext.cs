using Microsoft.EntityFrameworkCore;
using Task4.UserManagement.Models;

namespace Task4.UserManagement.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User>  Users { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}