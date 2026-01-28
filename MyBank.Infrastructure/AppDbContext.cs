using Microsoft.EntityFrameworkCore;
using MyBank.Domain;

namespace MyBank.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Conta> Contas { get; set; }
}