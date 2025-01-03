using Microsoft.EntityFrameworkCore;
using OnlineLibrarySystem_v1.Models.Entities;

namespace OnlineLibrarySystem_v1.Data
{
    public class LibraryDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks{ get; set; }

        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) {}

        /// <summary>
        /// Configure relationships
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API

            // The 'BorrowedBook' class has a navigation property 'Book' that links it to the Book entity.
            // This is the clue EF Core uses to infer the relationship.
            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.Book) // EF Core looks for the 'Book' navigation property in 'BorrowedBook' to identify the related entity.
                .WithMany(b => b.BorrowedBooks) // EF Core looks for the 'BorrowedBooks' navigation property in the 'Book' entity to establish the inverse relationship.
                .HasForeignKey(bb => bb.BookId);

            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.User)
                .WithMany(u => u.BorrowedBooks)
                .HasForeignKey(bb => bb.UserId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId);
        }
    }
}
