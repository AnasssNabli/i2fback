using I2FCONSEIL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace I2FCONSEIL.DATA
{
    public class AppDbContext : IdentityDbContext<Utilisateur, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add DbSets for your models
        public DbSet<Financier> Financiers { get; set; }
        public DbSet<Social> Sociaux { get; set; }
        public DbSet<Fiscal> Fiscaux { get; set; }
    }
}
