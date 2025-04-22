using CarService.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarService.Daos.UserDao;

public class UserDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public UserDbContext(DbContextOptions<UserDbContext> opts) : base(opts) { }
}