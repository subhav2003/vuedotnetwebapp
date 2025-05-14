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
        public DbSet<Bookmark> Bookmarks { get; set; }
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

            // Unique Member Email
            modelBuilder.Entity<Member>()
                .HasIndex(m => m.Email)
                .IsUnique();

            // Book ↔ Admin
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Admin)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            // Book ↔ Genre
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member ↔ Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member ↔ Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Member)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Book ↔ Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique Review: 1 review per member per book
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.MemberId, r.BookId })
                .IsUnique();

            // Book ↔ BookImage
            modelBuilder.Entity<BookImage>()
                .HasOne(img => img.Book)
                .WithMany(b => b.Images)
                .HasForeignKey(img => img.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart ↔ CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem ↔ Book
            modelBuilder.Entity<CartItem>()
                .HasOne(i => i.Book)
                .WithMany(b => b.CartItems)
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order ↔ OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem ↔ Book
            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Book)
                .WithMany()
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bookmark ↔ Member
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Member)
                .WithMany(m => m.Bookmarks)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Announcement ↔ Member (optional)
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Announcements) 
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.SetNull);



        }
    }
}
