using LoyaltySystem.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySystem.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LoyaltyProgram> Programs => Set<LoyaltyProgram>();
    public DbSet<MemberCard> Cards => Set<MemberCard>();
    public DbSet<LedgerEntry> Ledger => Set<LedgerEntry>();
    public DbSet<CardNumberCounter> CardNumberCounters => Set<CardNumberCounter>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<LoyaltyProgram>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(24);
            e.Property(x => x.Rounding).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.PriceBase).HasConversion<string>().HasMaxLength(16);
        });

        b.Entity<MemberCard>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OrganizationId, x.Number }).IsUnique();
            e.HasIndex(x => x.QToken).IsUnique();

            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

            e.HasOne(x => x.Program)
                .WithMany()
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OrganizationId, x.IdempotencyKey }).IsUnique();
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);

            e.HasOne(x => x.Card)
                .WithMany()
                .HasForeignKey(x => x.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CardNumberCounter>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrganizationId).IsUnique();
        });
    }
}
