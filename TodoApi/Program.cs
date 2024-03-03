// using Microsoft.AspNetCore.Http.Features;
// using TodoApi;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.OpenApi.Models;
// using System.Text.Json;
// using static TodoApi.ToDoDbContext;


// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddSingleton<Item>();
// builder.Services.AddSingleton<ToDoDbContext>();
// builder.Services.AddControllers();

// builder.Services.AddEndpointsApiExplorer();
// // builder.Services.AddSwaggerGen();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
// });

// var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")), ServiceLifetime.Singleton);

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll",
//         builder =>
//         {
//             builder.AllowAnyOrigin()
//             .AllowAnyMethod()
//             .AllowAnyHeader();
//         });
// });
// var app = builder.Build();

// app.UseSwagger();
// app.UseSwaggerUI(options =>
// {
//     options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
//     //options.RoutePrefix = string.Empty;
// });
// app.UseCors("AllowAll");
// // app.MapGet("/", context =>
// // {
// //     context.Response.Redirect("/swagger/index.html");
// //     return Task.CompletedTask;
// // });

// //app.MapGet("/api", () => "hello");
// app.MapGet("api/items", Get);
// app.MapPost("api/items", Post);
// app.MapPut("/api/items/{id}", Put);
// app.MapDelete("/api/items/{id}", Delete);

// app.Run();


// async Task Get(ToDoDbContext dbContext, HttpContext context)
// {
//     var items = await dbContext.Items.ToListAsync();
//     await context.Response.WriteAsJsonAsync(items);
// }
// async Task Post(ToDoDbContext dbContext, HttpContext context, Item item)
// {
//     dbContext.Items.Add(item);
//     await dbContext.SaveChangesAsync();
//     context.Response.StatusCode = StatusCodes.Status201Created;
//     await context.Response.WriteAsJsonAsync(item);
// }

// async Task Put(ToDoDbContext dbContext, HttpContext context, int id, Item item)
// {
//     // Validate the updatedItem parameter
//     if (item == null)
//     {
//         context.Response.StatusCode = StatusCodes.Status400BadRequest;
//         await context.Response.WriteAsync("Invalid item data");
//         return;
//     }

//     var existingItem = await dbContext.Items.FindAsync(id);
//     if (existingItem == null)
//     {
//         context.Response.StatusCode = StatusCodes.Status404NotFound;
//         await context.Response.WriteAsync($"Item with ID {id} not found");
//         return;
//     }

//     // Update the existing item with data from the updatedItem
//     if (item.Name != null)
//     {
//         existingItem.Name = item.Name;
//     }

//     existingItem.IsComplete = item.IsComplete;

//     await dbContext.SaveChangesAsync();
//     context.Response.StatusCode = StatusCodes.Status200OK;
//     // Return the updated item in the response body
//     await context.Response.WriteAsJsonAsync(existingItem);

// }
// async Task Delete(ToDoDbContext dbContext, HttpContext context, int id)
// {
//     var existingItem = await dbContext.Items.FindAsync(id);
//     if (existingItem == null)
//     {
//         context.Response.StatusCode = StatusCodes.Status404NotFound;
//         return;
//     }

//     dbContext.Items.Remove(existingItem);
//     await dbContext.SaveChangesAsync();
//     context.Response.StatusCode = StatusCodes.Status200OK;
// }

// // app.MapGet("/", () => {
// //     return ToDoDbContext.Items;
// // });
// // app.MapGet("/users/{userId}/books/{bookId}", 
// //     (int userId, int bookId) => $"The user id is {userId} and book id is {bookId}");
// // app.MapGet("/items", (ToDoDbContext dbContext) =>
// // {
// //     // Read items from the database
// //     var items = dbContext.Items.ToList();
// //     return JsonSerializer.Serialize(items);
// // });

// // app.MapPost("/", async (ToDoDbContext dbContext, Item newItem) =>
// // {
// //     // Add item to the database
// //     dbContext.Items.Add(newItem);
// //     await dbContext.SaveChangesAsync();

// //     return Results.Created("/", newItem); // Return the newly created item
// // });
// // app.MapPut("/", () => "This is a PUT");
// // app.MapDelete("/", () => "This is a DELETE");

// // // app.MapMethods("/options-or-head", new[] { "OPTIONS", "HEAD" }, 
// // //                           () => "This is an options or head request ");




// // builder.Services.AddSwaggerGen(c =>
// // {
// //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
// // });


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

