using TodoApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")), ServiceLifetime.Singleton);

builder.Services.AddCors(options =>
{ 
    options.AddPolicy("AllowSpecificOrigin",builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API"
    });
});
var app = builder.Build();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
// Use CORS
app.UseCors("AllowSpecificOrigin");
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/items", (ToDoDbContext dbContext) => GetTodos(dbContext));
app.MapPost("/items", (ToDoDbContext dbContext, Item item) => PostTodo(dbContext, item));
app.MapPut("/items/{id}", (ToDoDbContext dbContext, int id, bool isComplete) => PutTodo(dbContext, id, isComplete));
app.MapDelete("/items/{id}", (ToDoDbContext dbContext, int id) => DeleteTodo(dbContext, id));

app.Run();

List<Item> GetTodos(ToDoDbContext dbContext)
{
    return dbContext.Items.ToList();
}
Item PostTodo(ToDoDbContext dbContext, Item item)
{
    dbContext.Items.Add(item);
    dbContext.SaveChanges();
    return item;
}

Item PutTodo(ToDoDbContext dbContext, int id, bool isComplete)
{
    var existingItem = dbContext.Items.Find(id);
    if (existingItem != null)
    {
        existingItem.IsComplete = isComplete;
        dbContext.Items.Update(existingItem);
        dbContext.SaveChanges();
    }
    return existingItem;
}

void DeleteTodo(ToDoDbContext dbContext, int id)
{
    var item = dbContext.Items.Find(id);
    if (item != null)
    {
        dbContext.Items.Remove(item);
        dbContext.SaveChanges();
    }
}

