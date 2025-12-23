using Microsoft.EntityFrameworkCore;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Data;

/// <summary>
/// Database context for WAPS (Web-Based Audience Participation System)
/// Extends the existing game database with session and participant tables
/// </summary>
public class WAPSDbContext : DbContext
{
    public WAPSDbContext(DbContextOptions<WAPSDbContext> options) : base(options)
    {
    }

    // WAPS tables
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<FFFAnswer> FFFAnswers { get; set; }
    public DbSet<ATAVote> ATAVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HostName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });

        // Participant configuration
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.ConnectionId);
            
            entity.HasOne(e => e.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FFFAnswer configuration
        modelBuilder.Entity<FFFAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.ParticipantId).IsRequired();
            entity.Property(e => e.AnswerSequence).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.SessionId, e.QuestionId });
            entity.HasIndex(e => e.SubmittedAt);
            
            entity.HasOne(e => e.Session)
                .WithMany(s => s.FFFAnswers)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ATAVote configuration
        modelBuilder.Entity<ATAVote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.ParticipantId).IsRequired();
            entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SelectedOption).IsRequired().HasMaxLength(1);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.SubmittedAt);
            
            entity.HasOne(e => e.Session)
                .WithMany(s => s.ATAVotes)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
