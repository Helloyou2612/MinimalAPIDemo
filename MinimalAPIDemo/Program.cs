var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("MinimalApi");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connStr));

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

app.MapGet("/api/students", async (AppDbContext db) =>
{
    var students = await db.Students.ToListAsync();
    return Results.Ok(db.Students?.ToList());
}).WithName("GetStudents");
//Cách 1
app.MapGet("/api/students/{id}", async (AppDbContext db, int id) =>
{
    var student = await db.Students.FindAsync(id);
    return Results.Ok(student);
}).WithName("GetStudentId");

//Cách 2
app.MapPost("/api/students", async (AppDbContext db, Student student) 
    => await CreateStudent(db, student)).WithName("CreateStudent");

//Cách 3
app.MapPut("/api/students/{id}", async (AppDbContext db, int id, Student student) 
    => await new Enpoints().UpdateStudent(db, id, student)).WithName("UpdateStudent");

app.MapDelete("/api/students/{id}", async (AppDbContext db, int id) =>
{
    var emp = await db.Students.FindAsync(id);
    db.Students.Remove(emp);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

#endregion Students

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
}