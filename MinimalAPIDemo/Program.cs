var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("MinimalApi");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connStr));
//Registry DI
//builder.Services.AddTransient<IJurisRepository, JurisRepository>();

// Setup Services
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

#region Demo

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

#endregion Demo

#region Students

//Cách triển khải thứ nhất
app.MapGet("/api/students", async (AppDbContext db) =>
{
    var students = await db.Students.ToListAsync();
    return Results.Ok(db.Students?.ToList());
})
    .WithName("GetStudents")
    .AllowAnonymous();

app.MapGet("/api/students/{id}", async (AppDbContext db, int id) =>
{
    var student = await db.Students.FindAsync(id);
    return Results.Ok(student);
})
    .WithName("GetStudentId")//Add name for call from orther service (see method: CreateStudent)
    .AllowAnonymous();//Authentication

//Cách triển khải thứ 2
app.MapPost("/api/students", async (AppDbContext db, Student student) 
    => await CreateStudent(db, student))
    .WithName("CreateStudent");

//Cách triển khải thứ 3
app.MapPut("/api/students/{id}", async (AppDbContext db, int id, Student student) 
    => await new Enpoints().UpdateStudent(db, id, student))
    .WithName("UpdateStudent");

app.MapDelete("/api/students/{id}", async (AppDbContext db, int id) 
    => await new Enpoints().DeleteStudent(db, id))
    .WithName("DeleteStudent");

#endregion Students

// Start the Server
app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program
{
    private static async Task<IResult> CreateStudent(AppDbContext db, Student student)
    {
        db.Students.Add(student);
        await db.SaveChangesAsync();
        return Results.CreatedAtRoute("GetStudentId", new { id = student.StudentId }, student);
    }
}

public class Enpoints
{
    public async Task<IResult> UpdateStudent(AppDbContext db, int id, Student student)
    {
        db.Students.Update(student);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    public async Task<IResult> DeleteStudent(AppDbContext db, int id)
    {
        var emp = await db.Students.FindAsync(id);
        db.Students.Remove(emp);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}