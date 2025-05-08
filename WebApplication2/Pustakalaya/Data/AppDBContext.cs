using Microsoft.EntityFrameworkCore;
using Pustakalaya.Models;

namespace Pustakalaya.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<BookImage> BookImage { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>()
                .HasIndex(m => m.Email)
                .IsUnique();

            modelBuilder.Entity<Book>()
                .HasOne<Admin>(b => b.Admin)
                .WithMany()
                .HasForeignKey(b => b.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasOne<Genre>(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne<Member>(o => o.Member)
                .WithMany()
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne<Member>(r => r.Member)
                .WithMany()
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne<Book>(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<BookImage>()
                .HasOne(img => img.Book)
                .WithMany(b => b.Images)
                .HasForeignKey(img => img.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(i => i.Book)
                .WithMany()
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.Cascade);

        }
        
        

    }
}
