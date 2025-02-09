// using Microsoft.EntityFrameworkCore;
// using TodoApi;
// using Pomelo.EntityFrameworkCore.MySql;
// using Microsoft.AspNetCore.Http.Features;

// var builder = WebApplication.CreateBuilder(args);
    
// builder.Services.AddControllers();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigins", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });
// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
//     new MySqlServerVersion(new Version(8, 0, 2))));

// var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// } 
// app.MapGet("/", () => "Hello World!");

// app.MapGet("/tasks", async (ToDoDbContext db) =>
// {
//     return Results.Ok(await db.Items.ToListAsync());
// });

// app.MapPost("/tasks", async (ToDoDbContext db, User task) =>
// {
//     db.Items.Add(task);
//     await db.SaveChangesAsync();
//     return Results.Created($"/tasks/{task.Id}", task);
// });

// app.MapPut("/tasks/{id}", async (int id, ToDoDbContext db, User updatedTask) =>
// {
//     var task = await db.Items.FindAsync(id);
//     if (task is null)
//     {
//         return Results.NotFound();
//     }

//     task.Name = updatedTask.Name;
//     task.IsComplete = updatedTask.IsComplete;
    
//     await db.SaveChangesAsync();
//     return Results.NoContent(); 
// });

// app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
// {
//     var task = await db.Items.FindAsync(id);
//     if (task is null)
//     {
//         return Results.NotFound();
//     }

//     db.Items.Remove(task);
//     await db.SaveChangesAsync();
//     return Results.NoContent();
// });

// app.Run();
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת שירות ה-CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// הזרקת DbContext לאפליקציה
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql("Server=localhost;Port=3306;Database=db;User=root;Password=5708571cheviP@", 
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql")));

//builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
//     new MySqlServerVersion(new Version(8, 0, 2))));


var app = builder.Build();

// הפעלת Swagger 
app.UseSwagger(); 
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API V1"); 
    options.RoutePrefix = string.Empty;
});

// שימוש ב-CORS עבור כל הבקשות
app.UseCors("AllowAll");

app.MapGet("/", () => "Hello World!");

// שליפת כל המשימות
app.MapGet("/items", async (ToDoDbContext dbContext) =>
{
    var tasks = await dbContext.Items.ToListAsync();
    return tasks;
});

// הוספת משימה חדשה
app.MapPost("/items", async (ToDoDbContext dbContext,  Item task) =>
{
    dbContext.Items.Add(task);
    await dbContext.SaveChangesAsync();
    return Results.Ok($"המשימה '{task.Name}' נוספה בהצלחה.");
});

// עדכון משימה לפי ID
app.MapPut("/items/{id}", async (ToDoDbContext dbContext, int id, Item updatedTask) =>
{
    var task = await dbContext.Items.FindAsync(id);
    if (task == null) return Results.NotFound("המשימה לא נמצאה.");
    task.Name = updatedTask.Name;
    task.IsComplete = updatedTask.IsComplete;
    await dbContext.SaveChangesAsync();
    return Results.Ok($"המשימה בעמדה {id} עודכנה ל-{updatedTask}.");
});

// מחיקת משימה לפי ID
app.MapDelete("/items/{id}", async (ToDoDbContext dbContext, int id) =>
{
    var task = await dbContext.Items.FindAsync(id);
    if (task == null) return Results.NotFound("המשימה לא נמצאה.");
    dbContext.Items.Remove(task);
    await dbContext.SaveChangesAsync();
    return Results.Ok($"המשימה '{task.Name}' נמחקה.");
});

app.Run();