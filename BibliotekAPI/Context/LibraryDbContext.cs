using Microsoft.EntityFrameworkCore;
using BibliotekAPI.Models;

namespace BibliotekAPI.Context
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Loans)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(p => p.Name)
                    .IsRequired();

                entity.Property(p => p.Email)
                    .IsRequired();
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(p => p.Author)
                    .IsRequired();

                entity.Property(p => p.Title)
                    .IsRequired();

                entity.Property(p => p.ISBN)
                    .IsRequired();
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.Property(p => p.UserId)
                    .IsRequired();

                entity.Property(p => p.StartDate)
                    .IsRequired();

                entity.Property(p => p.EndDate)
                    .IsRequired();

                entity.Property(p => p.BookId)
                    .IsRequired();
            });


            // Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Alice Andersson",
                    Email = "alice@example.com",
                    Phone = "0701234567"
                },
                new User
                {
                    Id = 2,
                    Name = "Bob Berg",
                    Email = "bob@example.com",
                    Phone = "0707654321"
                }
            );

            // ---- BOOKS ----
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    ISBN = "9780132350884",
                    IsAvailable = true
                },
                new Book
                {
                    Id = 2,
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt",
                    ISBN = "9780201616224",
                    IsAvailable = true
                },
                new Book
                {
                    Id = 3,
                    Title = "Design Patterns",
                    Author = "Erich Gamma",
                    ISBN = "9780201633610",
                    IsAvailable = true
                }
            );

            // ---- LOANS ----
            modelBuilder.Entity<Loan>().HasData(
                new Loan
                {
                    Id = 1,
                    UserId = 1,
                    BookId = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 1, 15)
                },
                new Loan
                {
                    Id = 2,
                    UserId = 2,
                    BookId = 2,
                    StartDate = new DateTime(2025, 1, 5),
                    EndDate = new DateTime(2025, 1, 20)
                }
            );

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LoanHistory> LoanHistories { get; set; }


    }
}
