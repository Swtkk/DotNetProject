using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             // Relacja wiele-do-wielu: User -> Forum (Moderatorzy)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ModeratedForums)
                .WithMany(f => f.Moderators)
                .UsingEntity(j => j.ToTable("ForumModerators"));

            // Relacja jeden-do-wielu: Category -> Forum
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Forums)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja jeden-do-wielu: Forum -> Thread
            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Posts)
                .WithOne(t => t.Forum)
                .HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja jeden-do-wielu: Thread -> Message
            modelBuilder.Entity<Post>()
                .HasMany(t => t.Messages)
                .WithOne(m => m.Post)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja jeden-do-wielu: User -> Message
            modelBuilder.Entity<User>()
                .HasMany(u => u.Messages)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja jeden-do-wielu: User -> PrivateMessage (wysłane wiadomości)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SentMessages)
                .WithOne(pm => pm.Sender)
                .HasForeignKey(pm => pm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja jeden-do-wielu: User -> PrivateMessage (odebrane wiadomości)
            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedMessages)
                .WithOne(pm => pm.Receiver)
                .HasForeignKey(pm => pm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
    }
}