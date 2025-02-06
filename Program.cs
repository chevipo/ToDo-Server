using Microsoft.EntityFrameworkCore;
using TodoApi;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
    
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// הוספת שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

app.MapGet("/", () => "Hello World!");

app.MapGet("/tasks", async (ToDoDbContext db) =>
{
    return await db.NewTables.ToListAsync();
});

app.MapPost("/tasks", async (ToDoDbContext db, User task) =>
{
    db.NewTables.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks/{id}", async (int id, ToDoDbContext db, User updatedTask) =>
{
    var task = await db.NewTables.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound();
    }

    task.Name = updatedTask.Name;
    task.IsComplete = updatedTask.IsComplete;
    
    await db.SaveChangesAsync();
    return Results.NoContent(); 
});

app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var task = await db.NewTables.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound();
    }

    db.NewTables.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//
app.Run();
