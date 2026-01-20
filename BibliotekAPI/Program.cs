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



























			app.Run();
		}
	}
}
