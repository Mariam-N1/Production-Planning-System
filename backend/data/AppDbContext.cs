using Microsoft.EntityFrameworkCore;
using BackendProject.Models;

namespace BackendProject.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Shade> Shades { get; set; }
    public DbSet<Packing> Packings { get; set; }
    public DbSet<ProductShade> ProductShades { get; set; }
    public DbSet<ProductPacking> ProductPackings { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ProductionRun> ProductionRuns { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Delete a product and its shade links go with it.
        // The shade itself is untouched - other products may use it.
        b.Entity<ProductShade>()
            .HasOne(ps => ps.Product)
            .WithMany(p => p.ProductShades)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProductShade>()
            .HasOne(ps => ps.Shade)
            .WithMany(s => s.ProductShades)
            .HasForeignKey(ps => ps.ShadeId)
            .OnDelete(DeleteBehavior.Cascade);

        // The same product cannot be linked to the same shade twice.
        b.Entity<ProductShade>()
            .HasIndex(ps => new { ps.ProductId, ps.ShadeId })
            .IsUnique();

        b.Entity<ProductPacking>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.ProductPackings)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProductPacking>()
            .HasOne(pp => pp.Packing)
            .WithMany(pk => pk.ProductPackings)
            .HasForeignKey(pp => pp.PackingId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProductPacking>()
            .HasIndex(pp => new { pp.ProductId, pp.PackingId })
            .IsUnique();

        // One hex code belongs to exactly one shade.
        b.Entity<Shade>().HasIndex(s => s.ColorCode).IsUnique();

        // one account per email address
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // A run points at one product, one shade and one pack size.
        // WithMany() with no argument = the other side has no list, which is
        // what we want; Products should not carry a list of runs.
        b.Entity<ProductionRun>()
            .HasOne(r => r.Product).WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProductionRun>()
            .HasOne(r => r.Shade).WithMany()
            .HasForeignKey(r => r.ShadeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProductionRun>()
            .HasOne(r => r.Packing).WithMany()
            .HasForeignKey(r => r.PackingId)
            .OnDelete(DeleteBehavior.Cascade);

        // the screen always filters on this
        b.Entity<ProductionRun>().HasIndex(r => r.Status);

        b.Entity<Product>().HasIndex(p => p.Title);
        b.Entity<Product>().HasIndex(p => p.Category);
        b.Entity<Shade>().HasIndex(s => s.ShadeName);
        b.Entity<Packing>().HasIndex(p => p.PackingName);
    }
}
