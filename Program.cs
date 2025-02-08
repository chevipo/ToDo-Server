using Microsoft.EntityFrameworkCore;
using TodoApi;
using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
    new MySqlServerVersion(new Version(8, 0, 2))));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} 
app.MapGet("/", () => "Hello World!");

app.MapGet("/Items/{id}",async (int id, ToDoDbContext db) =>
{
    return await db.Items.FindAsync(id) is Item item ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/tasks", async (ToDoDbContext db, Item task) =>
{
    db.Items.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks/{id}", async (int id, ToDoDbContext db, Item updatedTask) =>
{
    var task = await db.Items.FindAsync(id);
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
    var task = await db.Items.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
