using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using reservasAppBackend;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", new CorsPolicyBuilder()
        .WithOrigins("http://127.0.0.1:5500") // Allow your frontend origin
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .Build());
});

// Add services to the container.
builder.Services.AddDbContext<BookingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// User CRUD operations
app.MapGet("/users", async (BookingContext db) =>
    await db.RegisteredUser.ToListAsync());

app.MapGet("/users/{id}", async (int id, BookingContext db) =>
    await db.RegisteredUser.Include(u => u.BookedDates).FirstOrDefaultAsync(u => u.Id == id) is RegisteredUser user
        ? Results.Ok(user)
        : Results.NotFound());

app.MapPost("/users", async (RegisteredUser user, BookingContext db) =>
{
    db.RegisteredUser.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users/{id}", async (int id, RegisteredUser inputUser, BookingContext db) =>
{
    var user = await db.RegisteredUser.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Username = inputUser.Username;
    user.UserPassword = inputUser.UserPassword;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (int id, BookingContext db) =>
{
    if (await db.RegisteredUser.FindAsync(id) is RegisteredUser user)
    {
        db.RegisteredUser.Remove(user);
        await db.SaveChangesAsync();
        return Results.Ok(user);
    }

    return Results.NotFound();
});

// Date CRUD operations

//app.MapGet("/dates/{id}", async (int id, BookingContext db) =>
//    await db.BookedDate.Include(d => d.RegisteredUser).FirstOrDefaultAsync(d => d.Id == id) is BookedDate date
//        ? Results.Ok(date)
//        : Results.NotFound());

app.MapGet("/dates", async (BookingContext db) =>
{
    try
    {
        var dates = await db.BookedDate
            .Include(d => d.RegisteredUser)
            .Select(d => new
            {
                d.Id,
                d.BookingDate,
                d.IdRegisteredUser,
                UserName = d.RegisteredUser.Username
            })
            .ToListAsync();
        return Results.Ok(dates);
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred while retrieving dates: {ex.Message}");
    }
});

app.MapPost("/dates", async (BookedDate date, BookingContext db) =>
{
    db.BookedDate.Add(date);
    await db.SaveChangesAsync();
    return Results.Created($"/dates/{date.Id}", date);
});

app.MapPut("/dates/{id}", async (int id, BookedDate inputDate, BookingContext db) =>
{
    var date = await db.BookedDate.FindAsync(id);
    if (date is null) return Results.NotFound();

    date.BookingDate = inputDate.BookingDate;
    date.IdRegisteredUser = inputDate.IdRegisteredUser;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

//app.MapDelete("/dates/{id}", async (int id, BookingContext db) =>
//{
//    if (await db.BookedDate.FindAsync(id) is BookedDate date)
//    {
//        db.BookedDate.Remove(date);
//        await db.SaveChangesAsync();
//        return Results.Ok(date);
//    }

//    return Results.NotFound();
//});

app.MapDelete("/dates/{id}", async (int id, BookingContext db) =>
{
    var date = await db.BookedDate.FindAsync(id);
    if (date == null)
    {
        return Results.NotFound();
    }

    try
    {
        db.BookedDate.Remove(date);
        await db.SaveChangesAsync();
        return Results.Ok(date);
    }
    catch (DbUpdateConcurrencyException)
    {
        // The date was not found in the database
        if (!await db.BookedDate.AnyAsync(d => d.Id == id))
        {
            return Results.NotFound();
        }
        else
        {
            throw; // Re-throw if it's a different concurrency issue
        }
    }
});



// Use CORS middleware
app.UseCors("AllowLocalhost");

app.Run();

