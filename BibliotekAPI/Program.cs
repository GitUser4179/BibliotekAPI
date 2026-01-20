using BibliotekAPI.Context;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using BibliotekAPI.Models;

namespace BibliotekAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddAuthorization();

			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();

			builder.Services.AddDbContext<LibraryDbContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.MapScalarApiReference();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapGet("/books", async (LibraryDbContext context) =>
			{
				var fetchedBooks = await context.Books.ToListAsync();
				return Results.Ok(fetchedBooks);
			});

			app.MapGet("/books/{id}", async (LibraryDbContext context, int id) =>
			{
				var fetchedBook = await context.Books.FirstOrDefaultAsync(b => b.Id == id);
				return Results.Ok(fetchedBook);
			});

			app.MapPost("/books", async (LibraryDbContext context, Book book) =>
			{
				await context.Books.AddAsync(book);
				await context.SaveChangesAsync();
				var fetchedBooks = await context.Books.ToListAsync();
				return Results.Ok(fetchedBooks);
			});

			app.MapPut("/books", async (LibraryDbContext context, Book updatedBook) =>
			{
				var fetchedBook = await context.Books.FirstOrDefaultAsync(b => b.Id == updatedBook.Id);
				// context.Entry(fetchedBook).CurrentValues.SetValues(updatedBook);
				
				fetchedBook.Author = updatedBook.Author;
				fetchedBook.Title = updatedBook.Title;
				fetchedBook.ISBN = updatedBook.ISBN;

				//context.Books.Update(fetchedBook);
				await context.SaveChangesAsync();

				var fetchedBooks = await context.Books.ToListAsync();
				return Results.Ok(fetchedBooks);
			});

			app.MapDelete("/books/{id}", async (LibraryDbContext context, int id) =>
			{
				// var fetchedBook = await context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
				
				if (await context.Books.FindAsync(id) is Book book)
				{
					context.Books.Remove(book);
					await context.SaveChangesAsync();
					return Results.NoContent();
				};

                return Results.NotFound();
			});

            //Endpoints for borrowers - CRUD operations

            app.MapGet("/borrowers", async (LibraryDbContext context) =>
            {
                var fetchedBorrowers = await context.Users.ToListAsync();

                return Results.Ok(fetchedBorrowers);
            });

            app.MapGet("/borrowers/{id}", async (int id, LibraryDbContext context) =>
            {
                var borrower = await context.Users.FirstOrDefaultAsync(b => b.Id == id);
                if (borrower == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(borrower);
            });

            app.MapPost("/borrowers", async (LibraryDbContext context, User newUser) =>
            {
                var added = await context.Users.AddAsync(newUser);

				await context.SaveChangesAsync();
				return Results.Ok(added.Entity);

            });

			app.MapPut("/borrowers", async (LibraryDbContext context, User user) =>
			{
				var fetchedUser = await context.Users.FirstOrDefaultAsync(b => b.Id == user.Id);
				if (fetchedUser == null)
				{
					return Results.NotFound();
				}
				fetchedUser.Name = user.Name;
				fetchedUser.Email = user.Email;
				fetchedUser.Phone = user.Phone;
				await context.SaveChangesAsync();
				return Results.Ok(fetchedUser);

            });
			app.MapDelete("/borrowers/{id}", async (LibraryDbContext context, int id) =>
			{
				var borrower = await context.Users.FindAsync(id);
				if (borrower == null)
				{
					return Results.NotFound();
				}
				context.Users.Remove(borrower);
				await context.SaveChangesAsync();
				return Results.NoContent();
			});


            //Endpoints for loans

            app.MapPost("/loans/user={userId}&book={bookId}", async (LibraryDbContext context, int bookId, int userId) =>
            {
                var foundBook = await context.Books.FindAsync(bookId);

                if (foundBook.IsAvailable)
                {
                    var loan = new Loan
                    {
                        BookId = foundBook.Id,
                        UserId = userId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(14) // 2 weeks loan period
                    };
                    foundBook.IsAvailable = false; // Mark the book as not available
                    await context.Loans.AddAsync(loan);
                    await context.SaveChangesAsync();
                    return Results.Ok(loan);
                }
                else
                    return Results.BadRequest("Book is not available for loan.");

            });

            app.MapGet("/loans/active", async (LibraryDbContext context) =>
            {
                var activeLoans = await context.Loans.
                    Where(l => l.ReturnDate == null)
                    .Include(l => l.Book)
                    .Include(l => l.User)
                    .ToListAsync();

                if (activeLoans == null || activeLoans.Count == 0)
                {
                    return Results.NotFound("No active loans found.");
                }
                else
                    return Results.Ok(activeLoans);
            });

            app.MapPost("/loans/{id}/return", async (LibraryDbContext context, int id) =>
            {
                var foundLoan = await context.Loans.Where(l => l.Id == id)
                .Include(b => b.Book)
                .Include(u => u.User)
                .FirstOrDefaultAsync();

                var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    if (foundLoan == null)
                    {
                        transaction.Rollback();
                        return Results.NotFound("Loan not found.");
                    }
                    else
                    {
                        foundLoan.ReturnDate = DateTime.UtcNow;
                        foundLoan.Book.IsAvailable = true; // Mark the book as available again
                        foundLoan.User.Loans.Remove(foundLoan); // Remove loan from user's active loans

                        await transaction.CommitAsync();
                        await context.SaveChangesAsync();

                        return Results.Ok(foundLoan);
                    }
                }
                catch
                {
                    // Both if or else complete the transaction, it is uneccessary.
                    // transaction.Rollback();
                    return Results.BadRequest();
                }
            });


            app.Run();


        }
    }
}
